### 最新的 NET9 SDK

https://github.com/dotnet/sdk/blob/main/documentation/package-table.md

### Nuget 每日构建源

```xml
<configuration>
  <packageSources>
    <add key="dotnet9" value="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet9/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

### 消除编译警告

```bash
.\vsregedit.exe set local HKCU Debugger\EngineSwitches ValidateDotnetDebugLibSignatures dword 0
```

### More

https://github.com/dotnet/aspnetcore/issues/54598