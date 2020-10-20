#wait for the SQL Server to come up
sleep 30s

echo "running set up script"
#run the setup script to create the DB and the schema in the DB
 /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P P@ssw0rd123 -d persondb