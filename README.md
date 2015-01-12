# Copiosis

This is the public repository for the CS469/470 Capstone Project for Copiosis at Portland State University.

## Server Setup

### Requirements
* Microsoft Windows Server 2008 or higher (any SKU)
* Microsoft IIS 7.0+
* SQL Server 2012 Express
* SQL Server 2012 Express Management Studio
* .NET 3.5


### Installation

1. Install Windows Server. It does not matter whether you use Web, Datacenter, Standard, or any other version -- but you are going to want the GUI, so select "Full Installation" **not "Server Core"**. The installer will reboot the computer.
2. Enable Remote Desktop **only** if incoming RDP is blocked by your firewall. Implementing a VPN within your firewall and using that to connect is the only way to secure RDC (Remote Desktop Connection) and RDP (Remote Desktop Protocol). Go to 'Control Panel' > 'System and  Maintenance' > 'System' and click on 'Remote settings'. Enable connecting from any version of Remote Desktop, unless you are going to administer only from a Windows 7 or higher computer with the latest patches to Remote Desktop. 
3. Select 'Roles' (Windows 2008 & 2008 R2) or 'Add Role' (2012 & 2012 R2) and add the Internet Information Service (IIS) role, following all defaults. We will customize the IIS in the second guide "IIS_Local_Development_Setup.pdf".
4. Install the SQL Server 2012 Express, following all defaults to create a database called SQLEXPRESS.
5. Install the SQL Server 2012 Express Management Studio. This installer does not ask any configuration questions.
6. Check for updates using Windows Update, installing **at minimum** all related to .NET runtimes, IIS, security patches, and Remote Desktop if used. There will be over 2GB of updates total including the previous.
7. Complete configuration using IIS_Local_Development_Setup.pdf
