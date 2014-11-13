using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Security.Principal;

public class CrmIdentity : IIdentity
{
	private string _name;
	private string _firstName;
	private string _lastName;
	private Guid _userId;
	private string _email;
	private List<string> _teams;
	private List<string> _roles;
	private bool _isAuthenticated;
	private string _authenticationType;

	public CrmIdentity()
	{
		_name = String.Empty;
		_firstName = String.Empty;
		_lastName = String.Empty;
		_userId = Guid.Empty;
		_email = String.Empty;
		_authenticationType = "LPA Dynamics CRM custom identity";
		_teams = new List<string>();
		_roles = new List<string>();
	}

	public void SetAuthenticated(bool authFlag)
	{
		_isAuthenticated = authFlag;
	}

	public void AddRole(string roleName)
	{
		if (!_roles.Contains(roleName))
		{
			_roles.Add(roleName);
		}
		else
		{
			throw new Exception("User is already a member of role " + roleName);
		}
	}

	public void RemoveRole(string roleName)
	{
		if (_roles.Contains(roleName))
		{
			_roles.Remove(roleName);
		}
		else
		{
			throw new Exception("User is not a member of role " + roleName);
		}
	}

	public bool InRole(string roleName)
	{
		if (_roles.Contains(roleName))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public void AddTeam(string teamName)
	{
		if (!_teams.Contains(teamName))
		{
			_teams.Add(teamName);
		}
		else
		{
			throw new Exception("User is already a member of team " + teamName);
		}
	}

	public void RemoveTeam(string teamName)
	{
		if (_teams.Contains(teamName))
		{
			_teams.Remove(teamName);
		}
		else
		{
			throw new Exception("User is not a member of team " + teamName);
		}
	}

	public bool InTeam(string teamName)
	{
		if (_teams.Contains(teamName))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}

	}

	public string Email
	{
		get
		{
			return _email;
		}
		set
		{
			_email = value;
		}

	}

	public string FirstName
	{
		get
		{
			return _firstName;
		}
		set
		{
			_firstName = value;
		}
	}

	public string LastName
	{
		get
		{
			return _lastName;
		}
		set
		{
			_lastName = value;
		}
	}

	public Guid UserId
	{
		get
		{
			return _userId;
		}
		set
		{
			_userId = value;
		}
	}

	public string[] Teams
	{
		get
		{
			return _teams.ToArray();
		}
	}

	public string[] Roles
	{
		get
		{
			return _roles.ToArray();
		}
	}

	public bool IsAuthenticated
	{
		get
		{
			return _isAuthenticated;
		}
	}

	public string AuthenticationType
	{
		get
		{
			return _authenticationType;
		}
	}
}