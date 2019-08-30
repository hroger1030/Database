# Database
This is a simple db interface assembly that I have had kicking around for a good ten 
years, I thought it might be worth sharing with people. It is a wrapper around a number
of ADO calls tied into an ORM object mapper that can automatically read recordsets into
POCO objects with coresponding fields. It also has the ability to load SQL metadat into
objects and create in memory representations of DB schemas.

I created a .net framework, .net core, and .net standard build of the assembly so
you should have the correct native version available regardless of what you are working
on.

There is a lot of stuff here that I have yet to fully document, so please drop me a line
 if you have any questions. 

## Sample Code

### Init object 

```
IDatabase db = new Database("sql connection string");
```

### Setup db call

```
// some input vars
int userId = 42;
string name = "DentArthurDent";

// set up parameters
var parameters = new SqlParameter[]
{
    new SqlParameter() {  SqlDbType = SqlDbType.Int, Value = userId, ParameterName = "UserId" },
    new SqlParameter() {  SqlDbType = SqlDbType.Varchar, Value = name, ParameterName = "Name", Size = 50 },	
};

Func<SqlDataReader, Dictionary<int, string>> processer = delegate (SqlDataReader reader)
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

// execute a store procedure 
var output = db.ExecuteQuerySp<Dictionary<int, string>("dbo.GetUserFriendList", parameters, processer);
```

