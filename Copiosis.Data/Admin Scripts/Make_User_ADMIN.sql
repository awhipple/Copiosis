/* Use this script to make a user have the ADMIN role which they need to
   gain access to /Admin pages. 
   values(UserID, 1), note: 1 is the admin role if you were to look at the
   dbo.webpages_Roles table */
insert into dbo.webpages_UsersInRoles
values((select userID from [dbo].[user] where username = 'kylevc'), 1)
