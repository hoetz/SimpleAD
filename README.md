SimpleAD
========
##Summary##
Active Directory meets the dynamic keyword
##Sample usage##
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
