# Database
This is a simple db interface assembly that I have had kicking around for a good ten 
years, I thought it might be worth sharing with people. It is a wrapper around a number
of ADO calls tied into an ORM object mapper that can automatically read recordsets into
POCO objects with coresponding fields. It also has the ability to load SQL metadat into
objects and create in memory representations of DB schemas.

I created a .net framework, .net core, and .net standard build of the assembly 
