# Database
This is a simple MS-SQL db interface object that I have been using for a number of years 
that I thought might be worth sharing with people. It is a wrapper around a number
of ADO calls tied into an ORM object mapper that can automatically read record sets into
POCO objects with corresponding fields. It also has the ability to load SQL meta data into
objects and create in memory representations of DB schemas.

I created this years ago because I saw things like Microsoft's Entity Framework that where
complex, bloated, and inefficient. The way to build a better ORM is not by trying to 
make something complex, but something lightweight and flexible. This is the result of 
those efforts.

I created a .net framework, .net core, and .net standard build of the assembly so
you should have the correct native version available regardless of what you are working
on. Please drop me a line if you have any questions. 

## Sample Code

The following code shows two basic use cases for the DAL. In the first, we will use a delegate function to 
read through a dataset manually, and map the results of a SQL stored procedure call into a c# collection.

The use case is, we have a SQL table that contains a list of employees and their jobs at a company. We
want to pass in a job name and get back all the employees that have that role. For the sake or brevity
error handling has been omitted.

The details of the SQL call aren't really important, but we can suppose that it is something like, "select Id,Name from Employees where JobTitle = '<parameter>'"

### Init object 

```
IDatabase db = new Database("sql connection string");
```

### Setup simple db call

```
// some input vars
string jobTitle = "Salesperson";

// set up parameters
var parameters = new SqlParameter[]
{
    new SqlParameter() {  SqlDbType = SqlDbType.Varchar, Value = JobTitle, ParameterName = "jobTitle", Size = 50 },	
};

Func<SqlDataReader, Dictionary<int, string>> processor = delegate (SqlDataReader reader)
{
    var output = new Dictionary<int, string>();

    while (reader.Read())
    {
        int id = (int)reader["Id"];
        string name = (string)reader["Name"];

        output.Add(id, name);
    }

    return output;
};

// execute a store procedure and return the results
var results = db.ExecuteQuerySp<Dictionary<int, string>>("[dbo].[GetEmployeeListByRole]", parameters, processor);
```

### Setup ORM mapper call

The second case shows how the DAL can automatically map data reader values into a c# class. It supposes the
same use case, but instead of a dictionary, we will be populating a list of Employee objects.

Note that the output from the SQL stored procedures matches the properties of the POCO class. This is 
important, as this is how the DAL automatically infers how to load data from the data reader. Also note
that the 'ShoeSize' property is skipped because it doesn't match a column returned by the data reader.

```
// define the Employee object container. It is a simple POCO without any business logic attached.
public class Employee
{
	public int Id {get;set;}
	public string Name {get;set;}	
	public int ShoeSize {get;set;}
}

// some input vars
string jobTitle = "Salesperson";

// set up parameters
var parameters = new SqlParameter[]
{
    new SqlParameter() {  SqlDbType = SqlDbType.Varchar, Value = jobTitle, ParameterName = "JobTitle", Size = 50 },	
};

// execute a store procedure
List<Employee> results = db.ExecuteQuerySp<Employee>("[dbo].[GetEmployeesByRole]", parameters);
```

This second use case is rather interesting, as it lets us simply generate containers that match the output of a stored 
procedure and not worry about the details of how the object is loaded. This model also is able to correctly cast 
to properties that are enumerated values, giving us a method to used strongly typed enumerations in our objects.


### Passing in a list of values via a datatable parameter

This is a slightly more advanced technique, desigened to allow you to pass in a collection
of values to a stored procedure via a table valued parameter. This is useful when you want to
insert, update or delete a collection of values in a single call.

```

// build parameter collection
var nameslist = new string[] { "Mal", "Jayne", "Wash", "River", "Book", "Zoe", "Kaylee", "Simon" };

// set up parameter
var parameters = new SqlParameter[]
{
    Database.ConvertObjectCollectionToParameter("valueList", "tblStringList", nameslist, "value"),
};

// execute a store procedure
var result = test.ExecuteNonQuerySp("[dbo].[BulkLoadExample]", parameters);
```

This particular example expects that the stored procedure accepts a user defined table parameter
as an argument. The table type might be defined as follows:

```
CREATE TYPE [dbo].[tblStringList] AS TABLE
(
	[Value] varchar(50) NULL
)
GO

CREATE PROCEDURE dbo.BulkLoadExample
(
	@valueList [tblStringList] READONLY
)
AS

insert Example ([name])
select [value]
from @valuelist

Return @@Rowcount
GO
```

### Running multiple queries in a single call

Occasionally, you might need to run multiple queries in a single call. This is useful when you want to
pull back several result sets over a single connection to incease performance. The following example
demonstrates this technique.

```
var queryList = new List<QueryData>()
{
    new QueryData()
    {
        Parameters = parameters,
        Query = "[dbo].[RotateAllFiggits]",
        StoredProcedure = true,
    },
    new QueryData()
    {
        Parameters = null,
        Query = "select * from [dbo].[Example]",
        StoredProcedure = false,
    },
    new QueryData()
    {
        Parameters = null,
        Query = "select * from [dbo].[Example] order by [name]",
        StoredProcedure = false,
    },
    new QueryData()
    {
        Parameters = null,
        Query = "select * from [dbo].[Example] where [shoesize] > 9",
        StoredProcedure = false,
    },

};

var result = test.ExecuteMultipleQueries(queryList);
```

This particular example is executing a mixed set of queries, some stored procedures, some not. The results
of each query will be placed in the output dataset, since it is possible to return differing datasets from
the queries. The results of the first query will be placed in the first table of the dataset, the second 
query in the second table, and so on.