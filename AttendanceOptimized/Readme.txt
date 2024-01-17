make sure to properly configure it to the desired bit architecture in default application pool that prioritizes 32/64 bits

x86 app on x64 pool will not run and result in unregistered dll crash (will be unable to be registered using regsvr32)
x64 on x86 will run but will be missing data or stuck on certain task due to unknown errors, best to err on the side of caution

.NET 4.6.1 works best, .NET CORE 6.0 for some reason also resulted in inconsistent data receiving (further informations needed)
