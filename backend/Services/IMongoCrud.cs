using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebPushApiDemo.Services
{
    public interface IMongoCrud
    {
        Task InsertRecord<T>(string table, T record);

        Task<List<T>> LoadRecords<T>(string table);
    }
}
