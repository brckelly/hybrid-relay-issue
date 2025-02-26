# Hybrid Relay Issue

When a .NET 8 (or 9) client POSTs to a .NET 4.x Framework listener, the call is being terminated too quickly.  This results in an error on the client side of connection reset by peer.  Adding a breakpoint to the listener side on the `.CloseAsync()` call, waiting for a few seconds and then continuing.  The client side will then read the response properly.

All other combinations work without interference.  A .NET 4.x Framework client will work just fine.  And both types of clients work when the listener is running .NET 8.

I have created a pair of sample applications that demonstrate this issue.

## Setup

There is an `appsettings.json` file for each project.  They should be configured like this:

```json
{
  "relayNamespace": "{name}.servicebus.windows.net",
  "connectionName": "{connection_name}",
  "keyName": "{sas_policy_name}",
  "key": "{sas_key}"
}
```

Where:

* relayNamespace - {name} is the name of the relay in Azure.
* connectionName - {connection_name} is the name of the Hybrid connection.
* keyName- {sas_policy_name} is the Shared Access Policy on the connection.  Make sure Listner has Listen Claims, and Client has Sender claims.
* key - {sas_key} is the primary key of the Shared Access Policy

## Running

Each project has two configurations.  One for running as .NET 4.7.2 and one as .NET 8.  The console will output the .NET version as the first line when it runs.
