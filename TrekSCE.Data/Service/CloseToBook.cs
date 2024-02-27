using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Dapper;
using System.Threading.Tasks;
using SCE.Data.Helper;
using SCE.Data.Dto;
using System.Linq;
using System.Collections;
using System.Configuration;

namespace SCE.Data.Service
{
    public class CloseToBook: BaseRepository
    {
       

        public async Task<ArrayList> GetSCEModelData(int modelId)
        {
            ArrayList SCEModelList = new ArrayList();
            using (connection = Get_Connection())
            {

                var param = new DynamicParameters();
                param.Add("ModelId", modelId, DbType.Int64, ParameterDirection.Input);
                using (var dataObj = await connection.QueryMultipleAsync("sp_SCEModelBinding", param, commandType: CommandType.StoredProcedure))
                {
                    SCEModelList.Add(dataObj.Read<SaleSlapseDaysDto>().ToList());
                    SCEModelList.Add(dataObj.Read<CloseToBookDto>().ToList());
                    SCEModelList.Add(dataObj.Read<SalesCycleExtensionDto>().ToList());
                }
            }
            return SCEModelList;
        }
        public async Task InsertUpdateSCE(int SectionId,int TableId,int TypeId,int Param1,int Param2)
        {
            try
            {
                using (connection = Get_Connection())
                {
                    var param = new DynamicParameters();
                    param.Add("SectionId", SectionId, DbType.Int64, ParameterDirection.Input);
                    param.Add("TableId", TableId, DbType.Int64, ParameterDirection.Input);
                    param.Add("TypeId", TypeId, DbType.Int64, ParameterDirection.Input);
                    param.Add("Param1", Param1, DbType.Int64, ParameterDirection.Input);
                    param.Add("Param2", Param2, DbType.Int64, ParameterDirection.Input);

                    await connection.ExecuteAsync("sp_SaveResetSCE", param, commandType: CommandType.StoredProcedure);
                  
                }

            }
            catch(Exception e)
            {
                throw;
            }
        }
    }
}
