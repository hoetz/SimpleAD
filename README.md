SimpleAD
========
##Summary##
Active Directory meets the dynamic keyword
##Sample usage##
			DirectoryConnection directoryCon = DirectoryConnection.Create();
            string filter = "flo*";
            dynamic Results = directoryCon
            	.Query(string.Format("(&(objectClass=user)(objectCategory=person)(samaccountname={0}))", filter));
            foreach (var user in Results)
            {
            	//see http://www.kouti.com/tables/userattributes.htm for AD attributes
                DateTime accountExpires = user.accountExpires;
                string name = user.name;
            }
