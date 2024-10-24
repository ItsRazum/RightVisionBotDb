namespace RightVisionBotDb.Types
{
    /*
    internal class RvUserPermissions
    {

        #region Properties

        public long RvUserId { get; }

        public Permission Permissions { get; set; }

        public Permission RemovedPermissions { get; set; }

        #endregion

        #region Constructors

        public RvUserPermissions(long userId = 0)
        {
            RvUserId = userId;
            CreatePermissions([]);
        }

        public RvUserPermissions(IEnumerable<Permission> permissions, long userId = 0)
        {
            RvUserId = userId;
            CreatePermissions(permissions);
        }

        public RvUserPermissions(long userId = 0, params Permission[] permissions)
        {
            RvUserId = userId;
            CreatePermissions(permissions);
        }

        public RvUserPermissions(Permission permissions, long userId = 0)
        {
            RvUserId = userId;
            Permissions = permissions;
        }

        private void CreatePermissions(IEnumerable<Permission> collection)
        {
            foreach (var permission in collection)
                Permissions |= permission;
        }

        #endregion

        #region Operators

        public static RvUserPermissions operator +(RvUserPermissions left, RvUserPermissions right)
        {
            foreach (var permission in PermissionsAsCollection(right.Permissions))
                left.Permissions |= permission;
            return left;
        }

        public static RvUserPermissions operator +(RvUserPermissions left, Permission right)
        {
            left.Permissions |= right;
            return left;
        }

        public static RvUserPermissions operator -(RvUserPermissions left, RvUserPermissions right)
        {
            foreach (var permission in PermissionsAsCollection(right.Permissions))
            {
                left.Permissions &= ~permission;
                left.RemovedPermissions |= permission;
            }

            return left;
        }

        public static RvUserPermissions operator -(RvUserPermissions left, Permission right)
        {
            left.Permissions &= ~right;
            left.RemovedPermissions |= right;
            return left;
        }

        #endregion

        #region Methods

        private static IEnumerable<Permission> PermissionsAsCollection(Permission permissions)
        {
            return Enum.GetValues(typeof(Permission))
                       .Cast<Permission>()
                       .Where(flag => (permissions & flag) == flag);
        }

        public static UserPermissions FromString(string s)
        {
            try
            {
                var parts = s.Split(':', 2);
                var userId = long.Parse(parts[0]);
                var value = parts[1];
                var collection = JsonConvert.DeserializeObject<List<Permission>>(value);
                if (collection != null)
                    return new RvUserPermissions(collection);
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

    }
    */
}
