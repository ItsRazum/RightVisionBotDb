using Newtonsoft.Json;
using RightVisionBotDb.Enums;
using RightVisionBotDb.Types;
using Serilog;
using System.Globalization;
using System.Text;

namespace RightVisionBotDb.Helpers
{
    internal class ConvertHelper
    {

        #region string <-> UserPermissions

        public static UserPermissions StringToPermissions(string value)
        {
            try
            {
                var parts = value.Split(':');
                var collection = JsonConvert.DeserializeObject<List<Permission>>(parts[0]);
                var removed = JsonConvert.DeserializeObject<List<Permission>>(parts[1]);
                return new UserPermissions(collection ?? [], removed ?? []);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Произошла ошибка при преобразовании строки в права доступа");
                throw;
            }
        }

        public static string PermissionsToString(UserPermissions value) 
        {
            StringBuilder sb = new("[ ");
            foreach (var perm in value)
                sb.Append($"\"{perm}\",");

            sb.Append(']');

            sb.Append(":[");
            foreach (var blockedPerm in value.Removed)
                sb.Append($"\"{blockedPerm}\",");

            sb.Append(']');
            return sb.ToString();
        }

        #endregion
    }
}
