# Publishing

## Create a self-signed certificate

```bash
New-SelfSignedCertificate -Type Custom -Subject "CN=Terrhsoft" -KeyUsage DigitalSignature -FriendlyName "SaveIt certificate" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")
```

Thumbprint will be used in the next step.

## Publish for Windows

```bash
dotnet publish .\SaveIt.App.UI -f net8.0-windows10.0.19041.0 -p:RuntimeIdentifierOverride=win10-x64
```