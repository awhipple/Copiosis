/* Use this script to reset a users password to a known good password. The
   known good password in this case will be the default user that has the password
   you want to reset to. You should replace the #### with the username of this user
   to fetch the password you want to reset the user $$$$ password with. */
update dbo.webpages_Membership
set Password = (select m.Password from [dbo].[user] u join dbo.webpages_Membership m on u.userID = m.UserId where u.username = '####')
where UserId = (select userID from [dbo].[user] u where u.username = '$$$$')