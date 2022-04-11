using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.SqlClient;

namespace ECSSO.Library
{
    #region input
    public class Beacon
    {
        public string BeaconUUID { get; set; }
        public int BeaconMajor { get; set; }
        public int BeaconMinor { get; set; }
        public int Power { get; set; }
    }
    #endregion
    #region output
    public class BeaconItem
    {
        public string Title { get; set; }
        public string URL { get; set; }
    }
    #endregion
    public class BeaconInstance
    {
        private GetStr gs = new GetStr();
        #region BeaconData CRUD
        public string SelectIdByBeacon(SqlConnection conn, Beacon obj)
        {
            string strId = "";
            string selectString = "SELECT Id "
                                + "From Beacon "
                                + "Where UUID = @BeaconUUID and Major = @BeaconMajor and Minor = @BeaconMinor;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@BeaconUUID", gs.CheckStringIsNotNull(obj.BeaconUUID));
                cmd.Parameters.AddWithValue("@BeaconMajor", obj.BeaconMajor);
                cmd.Parameters.AddWithValue("@BeaconMinor", obj.BeaconMinor);
                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            strId = reader["Id"].ToString();
                        }
                    }
                }
                catch
                {
                    strId = "";
                }
            }

            return strId;
        }
        public int UpdateBeaconPower(SqlConnection conn, string BeaconID, int Power)
        {
            int state = 0;
            if (gs.CheckStringIsNotNull(BeaconID) == "")
            {
                return 1;
            }

            string selectString = "UPDATE Beacon "
                                + " SET Power = @Power"
                                + " Where Id = @BeaconUUID;";
            using (SqlCommand cmd = new SqlCommand(selectString, conn))
            {
                cmd.Parameters.AddWithValue("@Power", Power);
                cmd.Parameters.AddWithValue("@BeaconUUID", BeaconID);
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    state = 1;
                }
            }
            return state;
        }
        #endregion
    }
}