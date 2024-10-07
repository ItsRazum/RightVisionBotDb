using Newtonsoft.Json;
using RightVisionBotDb.Permissions;
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

        private List<Permission> Removed
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

        public UserPermissions(IEnumerable<Permission> permissions, long userId = 0) => CreatePermissions(userId, permissions);

        public UserPermissions(long userId = 0, params Permission[] permissions) => CreatePermissions(userId, permissions);

        public static UserPermissions FromString(string s)
        {
            try
            {
                var parts = s.Split(':', 2);
                var userId = long.Parse(parts[0]);
                var value = parts[1];
                var collection = JsonConvert.DeserializeObject<List<Permission>>(value);
                if (collection != null)
                    return new UserPermissions(collection);
                else
                {
                    Log.Logger?.Error("Произошла ошибка при преобразовании строки в права доступа");
                    throw new NullReferenceException(nameof(value));
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Произошла ошибка при преобразовании строки в права доступа");
                throw;
            }
        }

        #endregion

        #region Operators

        public static UserPermissions operator +(UserPermissions left, UserPermissions right)
        {
            var combined = new UserPermissions(left.Collection);
            combined.AddList(right);
            return combined;
        }

        public static UserPermissions operator +(UserPermissions left, Permission right)
        {
            UserPermissions combined = new(left) { right };
            return combined;
        }

        public static UserPermissions operator -(UserPermissions left, UserPermissions right)
        {
            UserPermissions combined = new(left.Collection);
            combined.RemoveList(right);
            return combined;
        }

        public static UserPermissions operator -(UserPermissions left, Permission right)
        {
            UserPermissions combined = new(left.Collection);
            combined.Remove(right);
            return combined;
        }

        #endregion

        #region Methods

        public override string ToString()
        {
            StringBuilder sb = new("[ ");
            foreach (var perm in Collection)
                sb.Append($"\"{perm}\",");
            foreach (var blockedPerm in Removed)
                if (!string.IsNullOrEmpty(blockedPerm.ToString()))
                    sb.Append($"::\"{blockedPerm}\", ");

            sb.Append(']');
            return sb.ToString();
        }

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

        private void CreatePermissions(long userId, IEnumerable<Permission> collection)
        {
            Permissions = new(userId, new()
            {
                { "Permissions", new(collection) },
                { "Removed", new() }
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
