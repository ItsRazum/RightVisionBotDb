using RightVisionBotDb.Enums;
using System.Collections;

namespace RightVisionBotDb.Types
{
    public class UserPermissions : IEnumerable<Permission>
	{
		#region Properties

		public long RvUserId => Permissions.Key;

		public int Count => Collection.Count;

		public KeyValuePair<long, Dictionary<string, List<Permission>>> Permissions { get; set; } = new();

		public List<Permission> Collection
		{
			get => Permissions.Value["Permissions"];
			set
			{
				Permissions.Value["Permissions"] = value;
			}
		}

		public List<Permission> Removed
		{
			get => Permissions.Value["Removed"];
			set
			{
				Permissions.Value["Removed"] = value;
			}
		}

		#endregion

		#region Constructors

		public UserPermissions(long userId = 0) => CreatePermissions(userId, new List<Permission>());

		public UserPermissions(IEnumerable<Permission> permissions, long userId = 0) => CreatePermissions(userId, permissions.ToList());

		public UserPermissions(long userId = 0, params Permission[] permissions) => CreatePermissions(userId, [.. permissions]);

		public UserPermissions(IEnumerable<Permission> permissions, IEnumerable<Permission> removed, long userId = 0) => CreatePermissions(userId, permissions.ToList(), removed.ToList());

		#endregion

		#region Operators

		public static UserPermissions operator +(UserPermissions left, UserPermissions right)
		{
            var combinedPermissions = new UserPermissions(left.RvUserId);
            combinedPermissions.AddList(left.Collection);
            combinedPermissions.AddList(right.Collection);
            return combinedPermissions;
        }

		public static UserPermissions operator +(UserPermissions left, Permission right)
		{
            var combinedPermissions = new UserPermissions(left.RvUserId);
            combinedPermissions.AddList(left.Collection);
            combinedPermissions.Add(right);
            return combinedPermissions;
        }

		public static UserPermissions operator -(UserPermissions left, UserPermissions right)
		{
            var combinedPermissions = new UserPermissions(left.RvUserId);
            combinedPermissions.AddList(left.Collection);
            combinedPermissions.RemoveList(right);
            return combinedPermissions;
        }

		public static UserPermissions operator -(UserPermissions left, Permission right)
		{
            var combinedPermissions = new UserPermissions(left.RvUserId);
            combinedPermissions.AddList(left.Collection);
            combinedPermissions.Remove(right);
            return combinedPermissions;
        }

		#endregion

		#region Methods

		private void AddList(IEnumerable<Permission> list)
		{
			foreach (var permission in list)
				if (!Collection.Contains(permission))
                    Collection.Add(permission);
		}

		private void RemoveList(IEnumerable<Permission> list)
		{
            foreach (var permission in list)
                Collection.Remove(permission);
        }

		public void Add(Permission permission)
		{
			if (Collection.Contains(permission)) return;

			Collection.Add(permission);
			Removed.Remove(permission);
		}

		public void Remove(Permission permission)
		{
			Collection.Remove(permission);
			Removed.Add(permission);
		}

		private void CreatePermissions(long userId, List<Permission> collection, List<Permission>? removed = null)
		{
			Permissions = new(userId, new()
			{
				{ "Permissions", collection },
				{ "Removed", removed ?? [] }
			});
		}

		public bool Contains(Permission permission) => Collection.Contains(permission);

		#endregion

		#region IEnumerable Implementation

		public IEnumerator<Permission> GetEnumerator() => Collection.GetEnumerator();
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

		#endregion

	}
}
