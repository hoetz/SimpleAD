SimpleAD
========
##Summary##
Active Directory meets the dynamic keyword
##Sample usage##
```csharp
ActiveDirectory activeDirectory = ActiveDirectory.Setup();
string filter = "flo*";
dynamic Results = activeDirectory
                	.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", filter));
foreach (var user in Results)
{
	//see http://www.kouti.com/tables/userattributes.htm for AD attributes
	DateTime accountExpires = user.accountExpires;
	string name = user.name;
}


//Get your group memberships
ActiveDirectory activeDirectory = ActiveDirectory.Setup();
string sAMAccountName = "florian.hoetzinger";
dynamic Results = activeDirectory.Query(
			string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", sAMAccountName),
                	new string[] { "sAMAccountName", "sn", "memberOf" });
dynamic user = Results.First();
foreach (string parentItemDN in user.memberOf)
{
	//Do something with the distinguishedName of the group
}
