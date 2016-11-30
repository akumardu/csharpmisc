<#-----------------------------------------------------------------------------
  Beginning PowerShell Scripting for Developers
  Simple script that will just 'Do Something'

  Author: Robert C. Cain | @ArcaneCode | arcanecode@gmail.com
          http://arcanecode.com
 
  This module is Copyright (c) 2015 Robert C. Cain. All rights reserved.
  The code herein is for demonstration purposes. No warranty or guarentee
  is implied or expressly granted. 
  This module may not be reproduced in whole or in part without the express
  written consent of the author. 
-----------------------------------------------------------------------------#>

#-----------------------------------------------------------------------------#
# This very simple script is used to demonstrate being able
# to run scripts, work with execution policies, and the like.
#-----------------------------------------------------------------------------#

Set-Location 'C:\Users\amdubedy\Documents\github\csharpmisc\powershell\BasicPowershellCourse'

Get-ChildItem |
  Out-GridView -Wait

# SIG # Begin signature block
# MIIFuQYJKoZIhvcNAQcCoIIFqjCCBaYCAQExCzAJBgUrDgMCGgUAMGkGCisGAQQB
# gjcCAQSgWzBZMDQGCisGAQQBgjcCAR4wJgIDAQAABBAfzDtgWUsITrck0sYpfvNR
# AgEAAgEAAgEAAgEAAgEAMCEwCQYFKw4DAhoFAAQUWAfPyIgpxXizNUqKgDLbi0R0
# mSagggNCMIIDPjCCAiqgAwIBAgIQyVkdEwGpOoNIpU/dt5h0dDAJBgUrDgMCHQUA
# MCwxKjAoBgNVBAMTIVBvd2VyU2hlbGwgTG9jYWwgQ2VydGlmaWNhdGUgUm9vdDAe
# Fw0xNjExMTgyMjU2MzJaFw0zOTEyMzEyMzU5NTlaMBoxGDAWBgNVBAMTD1Bvd2Vy
# U2hlbGwgVXNlcjCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBANATrUGP
# 13BHqcLmJaGSTE0/bIixqv6/KgtvIBuAIRKupfsaUxvYa5Z/+Ye8rFG1LG1mVZLb
# XB1qdm5a1CbrlDTKgAvED/cK5Kjsp266n06LhPLpKOMmJAe4eWFVLcQhjXIQ9+tF
# XXrrs/2R2glgjIsIOvJBq82CqP9UxCrEPqhGcR0oLabfoK1aHm2qSmUuz7edosLm
# IFvMqn3FxqcBJg7Tf1te349wr5jNMejFnvTVdfiec5+n352HuCQPXTxkrdBaI+3D
# 4YZwTV1BMa4OMumwsNSR6cu+VU8Rt+jMIP5G5+OWfu7apv0GwqcFkgperJL2cw5u
# P8VjstMDr5NUKhsCAwEAAaN2MHQwEwYDVR0lBAwwCgYIKwYBBQUHAwMwXQYDVR0B
# BFYwVIAQO4K9YnO1U9UzkEFmy803qaEuMCwxKjAoBgNVBAMTIVBvd2VyU2hlbGwg
# TG9jYWwgQ2VydGlmaWNhdGUgUm9vdIIQvha1VqWd84xJ2fTrxrnvyzAJBgUrDgMC
# HQUAA4IBAQDA/2u7qHz7GbzJlfrISQv7XcjYfY05UaO+7LxkLGUAZQ3hhmhRxEg/
# A5uycyplma5fEGiw7/zrkBVgSUseLpEtsM3O40tI86/SBh902BLbKRVSbswLwERb
# iExpeMiU3RcXDcaIvXQa2pQF/xKXikl3iCDngW71cZQ/vGHwKyZiqzW2KE0okug+
# Xa1lu8SGTJRgVEAZ6QkqLnavXBYVtm8oZJQgf3Lo853GLlR3K0a7dv1cdLRSBdvb
# tf4xgHuJ5D9ytwE4AwHDX9G0BrcMDL7pp28Aoa+nWYuuqqqSgBbrbh5DrYgGdBDO
# sJdAbAbp5tZMj5i58XsP2yH0mbevdyBQMYIB4TCCAd0CAQEwQDAsMSowKAYDVQQD
# EyFQb3dlclNoZWxsIExvY2FsIENlcnRpZmljYXRlIFJvb3QCEMlZHRMBqTqDSKVP
# 3beYdHQwCQYFKw4DAhoFAKB4MBgGCisGAQQBgjcCAQwxCjAIoAKAAKECgAAwGQYJ
# KoZIhvcNAQkDMQwGCisGAQQBgjcCAQQwHAYKKwYBBAGCNwIBCzEOMAwGCisGAQQB
# gjcCARUwIwYJKoZIhvcNAQkEMRYEFEuZOEnleM6W73GYDWSNUrhEPRWwMA0GCSqG
# SIb3DQEBAQUABIIBAGhvPRhhVbxdWz/BKVBHDiF0WXO21z+mc5hE4VCcibB7Xt93
# Vddt0Yj7kJ8vTBcdlvP/8KVY2HLyHpGtGPJ4GWAgcsx4GUvVWlFX14bGKqpluLvW
# BiIHZXEbVz//wG4TfCBW24uRe/7MOLW04BAqPOEF88m6rPhUrBeNcU4sbAXSsu0t
# mkv9aP8RvUkqA1yuGAl82gSx5CdAxn4kQCuKEORzh0Y3dqJx1SUj2Pdl3+yF8mKl
# yOq+/jmRJvRNaNrYMOc0vXbLy0+Zzm8AdwKWRjPBtpf+kLmpI8nRqYFNvLB//+zU
# S5I2zuQkKJO2D2hrEZqakRfpFQrxRPGay+FKtYU=
# SIG # End signature block
