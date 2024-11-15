# service-tester

Build to deploy on linux with: `dotnet publish -c Release -r linux-x64 --self-contained
`

Usage: `service-tester <sql | mongo | redis> "<connection-string>"`