/* Use this script to make change the token needed for users to signup */
update dbo.location
set signupKey = '########'		-- Replace ##### with whatever token you want to use
where neighborhood = '$$$$$$'	-- Replace $$$$$ with the name of the neighborhood you are updating