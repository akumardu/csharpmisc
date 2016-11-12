<#-----------------------------------------------------------------------------
  Beginning PowerShell Scripting for Developers
  Working With Objects

  Author: Robert C. Cain | @ArcaneCode | arcanecode@gmail.com
          http://arcanecode.com
 
  This module is Copyright (c) 2015 Robert C. Cain. All rights reserved.
  The code herein is for demonstration purposes. No warranty or guarentee
  is implied or expressly granted. 
  This module may not be reproduced in whole or in part without the express
  written consent of the author. 
-----------------------------------------------------------------------------#>

#-----------------------------------------------------------------------------#
# Demo 0 -- Object Oriented Terminology
#-----------------------------------------------------------------------------#
<#
                                  class = blueprints
                             properties = describe
                    methods (functions) = actions
                                 object = house
             instantiating a new object = building a new house
each object is an instance of the class = each house is a copy of the blueprint


#>




#-----------------------------------------------------------------------------#
# Demo 1 -- Create a new object 
# This is the most common method of creating objects
#-----------------------------------------------------------------------------#
function Create-Object ($Schema, $Table, $Comment)
{
  # Build a hash table with the properties
  $properties = [ordered]@{ Schema = $Schema
                   Table = $Table
                   Comment = $Comment
                 }

  # Start by creating an object of type PSObject
  $object = New-Object –TypeName PSObject -Property $properties

  # Return the newly created object
  return $object
}

$myObject = Create-Object -Schema "MySchema" -Table "MyTable" -Comment "MyComment"
$myObject

# Display in text. Note because it is an object need to wrap in $() to access a property
"My Schema = $($myObject.Schema)"

$myObject.Schema = "New Schema"
$myObject.Comment = "New Comment"
$myObject

















#-----------------------------------------------------------------------------#
# Demo 2 -- Create a new object by adding properties one at a time
# In the previous demo a property hash table was used to generate the object
# Behind the scenes it does the equivalent of what this function does
#-----------------------------------------------------------------------------#
function Create-Object ($Schema, $Table, $Comment)
{
  # Start by creating an object of type PSObject
  $object = New-Object –TypeName PSObject

  # Add-Member by passing in input object
  Add-Member -InputObject $object `
             –MemberType NoteProperty `
             –Name Schema `
             –Value $Schema

  # Alternate syntax, pipe the object as an input to Add-Member
  $object | Add-Member –MemberType NoteProperty `
                       –Name Table `
                       –Value $Table
  
  $object | Add-Member -MemberType NoteProperty `
                       -Name Comment `
                       -Value $Comment

  return $object
}

$myObject = Create-Object -Schema "MySchema" -Table "MyTable" -Comment "MyComment"
$myObject

# Display in text. Note because it is an object need to wrap in $() to access a property
"My Schema = $($myObject.Schema)"

$myObject.Schema = "New Schema"
$myObject.Comment = "New Comment"
$myObject


















#-----------------------------------------------------------------------------#
# Demo 3 -- Add alias for one of the properties
#-----------------------------------------------------------------------------#
Clear-Host
Add-Member -InputObject $myObject `
           -MemberType AliasProperty `
           -Name 'Description' `
           -Value 'Comment' `
           -PassThru
"Comment......: $($myObject.Comment)"
"Description..: $($myObject.Description)"



# Demo 3 -- Add script block to object
Clear-Host
$block = { 
           $fqn = $this.Schema + '.' + $this.Table 
           return $fqn
         }

Add-Member -InputObject $myObject `
           -MemberType ScriptMethod `
           -Name 'FullyQualifiedName' `
           -Value $block `
           -PassThru

# Parens are very important, without it will just display the function
$myObject.FullyQualifiedName()  




















#-----------------------------------------------------------------------------#
# Demo 4 -- Script block with parameters
#-----------------------------------------------------------------------------#
Clear-Host
$block = { 
           param ($DatabaseName)
           $dqn = "$DatabaseName.$($this.Schema).$($this.Table)"
           return $dqn
         }

Add-Member -InputObject $myObject `
           -MemberType ScriptMethod `
           -Name 'DatabaseQualifiedName' `
           -Value $block `
           -PassThru

# Parens are very important, without it will just display the function
$myObject.DatabaseQualifiedName('MyDBName')  

















#-----------------------------------------------------------------------------#
# Demo 5 -- Script Property
#-----------------------------------------------------------------------------#
# These are analogues to properties in C#, with a Getter and Setter function
Clear-Host

# Add a property we can work with
Add-Member -InputObject $myObject `
           –MemberType NoteProperty `
           –Name AuthorName `
           –Value 'No Author Name'

# This defines the GET for this property
$getBlock = { return $this.AuthorName }

# This defines the SET. Adding a simple check for the name
$setBlock = { 
              param ( [string]$author )
                            
              if($author.Length -eq 0)
              { $author = 'Robert C. Cain, MVP' }
              
              $this.AuthorName = $author
            }

# Now add the custom Get/Set ScriptProperty to the member
Add-Member -InputObject $myObject `
           -MemberType ScriptProperty `
           -Name Author `
           -Value $getBlock `
           -SecondValue $setBlock

# Demo its use when passing as value
$myObject.Author = 'ArcaneCode'
"`$myObject.Author now equals $($myObject.Author )"

# Now pass in nothing to see the setter functionality kicking in
$myObject.Author = ''
$myObject.Author

# Unfortunately the original property is still available, and thus
# the custom get/set can be bypassed
$myObject.AuthorName = 'Evil Author'
$myObject.Author                       # Author reflects value of AuthorName















#-----------------------------------------------------------------------------#
# Demo 6 -- Set default properties
#-----------------------------------------------------------------------------#
Clear-Host

# When just running the object, it displays all properties
$myObject

# If you have a lot, this can get overwhelming. Instead you can define a
# default set to display.

# Define the property names in an array
$defaultProperties = 'Schema', 'Table', 'Comment', 'Author'

# Create a property set object and pass in the array 
$defaultPropertiesSet `
  = New-Object System.Management.Automation.PSPropertySet(`
      ‘DefaultDisplayPropertySet’ `
      ,[string[]]$defaultProperties `
      )

# Create a PS Member Info object from the previous property set object
$members `
  = [System.Management.Automation.PSMemberInfo[]]@($defaultPropertiesSet)

# Now add to the object
$myObject | Add-Member MemberSet PSStandardMembers $members

# Now the object will just display the default list in standard output
$myObject

# Little easier to read in a list
$myObject | Format-List

# To display all properties, pipe through format-list with wild card for property
$myObject | Format-List -Property *













#-----------------------------------------------------------------------------#
# Demo 7 - Create a class from .Net Code and call a static method
#-----------------------------------------------------------------------------#

# Load the class definition into a variable
$code = @"
using System;

public class BPSDObjectStatic
{
  public static string composeFullName(string pSchema, string pTable)
  {
    string retVal = "";  

    retVal = pSchema + "." + pTable;

    return retVal;

  } // public static string composeFullName
} // class BPSDObjectStatic

"@

# Add a new type definition based on the code
Add-Type -TypeDefinition $code `
         -Language CSharpVersion3 

# Call the static method of the object
$mySchema = "dbo"
$myTable = "ArcaneCode"
$result = [BPSDObjectStatic]::composeFullName($mySchema, $myTable)
$result


#-----------------------------------------------------------------------------#
# Demo 8 - Instantiate an object from .Net Code in embedded code
#-----------------------------------------------------------------------------#

$code = @"
using System;

public class BPSDObjectEmbedded
{
  public string SomeStringName;

  public string composeFullName(string pSchema, string pTable)
  {
    string retVal = "";  

    retVal = pSchema + "." + pTable;

    return retVal;

  } // public string composeFullName
} // class BPSDObjectEmbedded

"@

# Add a new type definition based on the code
Add-Type -TypeDefinition $code `
         -Language CSharpVersion3 

# Instantiate a new version of the object
$result = New-Object -TypeName BPSDObjectEmbedded

# Set and display the property
$result.SomeStringName = "Temp"
$result.SomeStringName

# Call the method
$result.composeFullName($mySchema, $myTable)
















#-----------------------------------------------------------------------------#
# Demo 9 - Create object from .Net Code in an external file
#-----------------------------------------------------------------------------#

# Set the folder where the CS file is
$assyPath = "C:\PS\Beginning PowerShell Scripting for Developers\demo\"

# Path and File Name
$file = "$($assyPath)bpsd-m07.cs"

# Load the contents of the file into a variable
$code = Get-Content $file | Out-String

# Add a new type definition based on the code
Add-Type -TypeDefinition $code `
         -Language CSharpVersion3 

# Call the static method of the object
$mySchema = "dbo"
$myTable = "ArcaneCode"
$result = [BPSDObjectExternal]::composeFullName($mySchema, $myTable)
$result















#-----------------------------------------------------------------------------#
# Demo 10 - Add to an existing object
#-----------------------------------------------------------------------------#

# Define the custom script method
$script = { 
            $retValue = "Unknown"

            if($this.Extension -eq '.ps1')
            {
              $retValue = 'Script'
            }
            else
            {
              $retValue = 'Not A Script'
            }

            return $retValue
          }

# Load a variable with a collection of file objects
Set-Location "C:\PS\Beginning PowerShell Scripting for Developers\demo"
$items = Get-ChildItem

# Add script property to the each file object in bulk
$items | Add-Member -MemberType ScriptMethod `
                    -Name 'ScriptType' `
                    -Value $script 


# Now, for illustrative purposes, add another property to 
# each item using a foreach loop
$itemCount = 0
foreach($item in $items)
{
  $itemCount++

  # Add a note property, setting it to the current item counter
  # Could have also used $item | Add-Member...
  Add-Member -InputObject $item `
             –MemberType NoteProperty `
             –Name ItemNumber `
             –Value $itemCount
  
  # Display the results of the file object with the new 
  # property and script (function) added
  "$($item.ItemNumber): $($item.Name) = $($item.ScriptType())"
}












#-----------------------------------------------------------------------------#
# Demo 11 - Serializing an Object
#-----------------------------------------------------------------------------#

  # Create a simple object
  $mySiteProperties = [ordered]@{ 
                                  WebSite = 'ArcaneCode'
                                  URL = 'http://arcanecode.com'
                                  Twitter = '@ArcaneCode'
                                }

  # Convert it to an object
  $mySite = New-Object –TypeName PSObject -Property $mySiteProperties
  
  # Show the object
  $mySite
  
  # Save the object to a file and 
  $savedDataFile = 'C:\PS\Beginning PowerShell Scripting for Developers\demo\mySite.xml'
  $mySite | Export-Clixml $savedDataFile
  psedit $savedDataFile 

  # Now grab the saved object and recreate in a different variable
  $newMySite = Import-Clixml $savedDataFile
  $newMySite










#-----------------------------------------------------------------------------#
# The following section requires PowerShell Version 5
# PowerShell V5 is included in Windows 10
# Free upgrade to previous versions of Windows
# It is included as part of the Windows Management Framework 5.0
# Search for "powershell version 5 download" to find the most current
# version of the WMF (aka PowerShell).
#-----------------------------------------------------------------------------#


#region Enum

#-----------------------------------------------------------------------------#
# Show Enums in PS
#-----------------------------------------------------------------------------#

# Define the valid values for the enum
Enum MyTwitters
{
  Pluralsight
  ArcaneCode
  N4IXT
}

# Note when typing the last : will trigger intellisense!
$tweet = [MyTwitters]::ArcaneCode
$tweet

# See if they picked something valid
[enum]::IsDefined(([MyTwitters]), $tweet)

# Set it to something invalid and see if it passes as an enum
$tweet = 'Invalid'
[enum]::IsDefined(([MyTwitters]), $tweet)

#endregion Enum





#region Basic Class



#-----------------------------------------------------------------------------#
# Basic Class
#-----------------------------------------------------------------------------#
  
Class Twitterer
{
  # Create a property
  [string]$TwitterHandle
  
  # Create a property and set a default value
  [string]$Name = 'Robert C. Cain'

  # Function that returns a string
  [string] TwitterURL()
  {
    $url = "http://twitter.com/$($this.TwitterHandle)"
    return $url
  }

  # Function that has no return value
  [void] OpenTwitter()
  {
    Start-Process $this.TwitterURL()
  }

}

$twit = [Twitterer]::new()
$twit.TwitterHandle = 'ArcaneCode'
$twit.TwitterHandle
  
# See default property value
$twit.Name

# Override default value
$twit.Name = 'Robert Cain'
$twit.Name

$myTwitter = $twit.TwitterURL()
$myTwitter

$twit.OpenTwitter()

#endregion Basic Class



















#region Advanced Class

#-----------------------------------------------------------------------------#
# Advanced Class
#   Constructors
#   Overloaded Methods
#-----------------------------------------------------------------------------#

Class TwittererRedux
{
  # Default Constructor
  TwittererRedux ()
  {
  }

  # Constructor passing in Twitter Handle
  TwittererRedux ([string]$TwitterHandle)
  {
    $this.TwitterHandle = $TwitterHandle
  }

  # Constructor passing in Twitter Handle and Name
  TwittererRedux ([string]$TwitterHandle, [string]$Name)
  {
    $this.TwitterHandle = $TwitterHandle
    $this.Name = $Name
  }

  # Create a property
  [string]$TwitterHandle
  
  # Create a property and set a default value
  [string]$Name = 'Robert C. Cain'

  # Static Properties
  static [string] $Version = "2015.09.23.001"

  # Function that returns a string
  [string] TwitterURL()
  {
    $url = "http://twitter.com/$($this.TwitterHandle)"
    return $url
  }

  # Overloaded Function that returns a string
  [string] TwitterURL($twitterHandle)
  {
    $url = "http://twitter.com/$($twitterHandle)"
    return $url
  }

  # Function that has no return value
  [void] OpenTwitter()
  {
    Start-Process $this.TwitterURL()
  }

  # Can launch a twitter page without instantiating the class
  static [void] OpenTwitterPage([string] $TwitterHandle)
  {
    $url = "http://twitter.com/$($TwitterHandle)"
    Start-Process $url
  }

}

# Create a class using default constructor
$twitDefault = [TwittererRedux]::new()

# Display without assigning
"TwitterHandle = $($twitDefault.TwitterHandle)"

# Now assign and display again
$twitDefault.TwitterHandle = 'Pluralsight'
"TwitterHandle = $($twitDefault.TwitterHandle)"

# Show version one of TwitterURL
"URL = $($twitDefault.TwitterURL())"

# Show overloaded version
"URL = $($twitDefault.TwitterURL('ArcaneCode'))"

# Note the overload does not change the TwitterHandle
# (But only because I did not code it to do so)
$twitDefault.TwitterHandle 

# Create a new instance using the second constructor
$twitAdvanced = [TwittererRedux]::new('N4IXT')

# Display without assigning - Should have what was passed in constructor
"TwitterHandle = $($twitAdvanced.TwitterHandle)"

# Create yet another instance using the third constructor
$twitAdvanced2 = [TwittererRedux]::new('ArcaneCode', 'R Cain')
$twitAdvanced2.TwitterHandle
$twitAdvanced2.Name

# Static Value - Can be called without initializing the class
[TwittererRedux]::Version

# Use the static method
[TwittererRedux]::OpenTwitterPage('N4IXT')


#endregion Advanced Class


