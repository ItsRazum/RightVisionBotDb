using Newtonsoft.Json;
using RightVisionBotDb.Enums;
using Serilog;
using System.Collections;
using System.Text;

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
			left.AddList(right);
			return left;
		}

		public static UserPermissions operator +(UserPermissions left, Permission right)
		{
			left.Add(right);
			return left;
		}

		public static UserPermissions operator -(UserPermissions left, UserPermissions right)
		{
			left.RemoveList(right);
			return left;
		}

		public static UserPermissions operator -(UserPermissions left, Permission right)
		{
			left.Remove(right);
			return left;
		}

		#endregion

		#region Methods

		private void AddList(IEnumerable<Permission> list)
		{
			List<Permission> combinedList = new(Collection);

			foreach (var permission in list)
				if (!combinedList.Contains(permission))
					combinedList.Add(permission);

			Collection = combinedList;
		}

		private void RemoveList(IEnumerable<Permission> list)
		{
			List<Permission> combinedList = new(list);
			foreach (var permission in combinedList.Where(permission => !combinedList.Contains(permission)))
				combinedList.Remove(permission);

			Collection = combinedList;
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
