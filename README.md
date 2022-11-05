# Database
This is a simple db interface assembly that I have had kicking around for a good ten 
years, I thought it might be worth sharing with people. It is a wrapper around a number
of ADO calls tied into an ORM object mapper that can automatically read record sets into
POCO objects with corresponding fields. It also has the ability to load SQL meta data into
objects and create in memory representations of DB schemas.

I created a .net framework, .net core, and .net standard build of the assembly so
you should have the correct native version available regardless of what you are working
on.

There is a lot of stuff here that I have yet to fully document, so please drop me a line
 if you have any questions. 

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