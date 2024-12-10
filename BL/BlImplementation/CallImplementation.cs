using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlImplementation
{
    using BlApi;
    using DalApi;
    using BO;
    using System.Linq;
    using BlApi.BlApi;
    using DO;

    internal class CallImplementation : ICall
    {
        private readonly IDal _dal = DalApi.Factory.Get();

        public int[] GetCallsCountByStatus()
        {
            return _dal.Call.ReadAll()
                .GroupBy(c => c.Status)
                .OrderBy(g => g.Key)
                .Select(g => g.Count())
                .ToArray();
        }

        public IEnumerable<CallInList> GetCallsList(CallSortField? filterField, object? filterValue, CallSortField? sortField)
        {
            var calls = _dal.Call.ReadAll();

            if (filterField.HasValue && filterValue != null)
                calls = calls.Where(c => c.GetType().GetProperty(filterField.Value.ToString())?.GetValue(c)?.Equals(filterValue) == true);

            var sortedCalls = sortField switch
            {
                CallSortField.Id => calls.OrderBy(c => c.Id),
                CallSortField.Status => calls.OrderBy(c => c.Status),
                _ => calls
            };

            return sortedCalls.Select(c => new CallInList
            {
                Id = c.Id,
                Details = c.Details,
                Status = c.Status
            });
        }

        public Call GetCallDetails(int id)
        {
            var call = _dal.Call.Read(id);
            if (call == null)
                throw new KeyNotFoundException($"Call with ID {id} not found.");

            return new Call
            {
                Id = call.Id,
                Details = call.Details,
                Status = call.Status
            };
        }

        public void UpdateCall(Call call)
        {
            var updatedCall = new DO.Call
            {
                Id = call.Id,
                Details = call.Details,
                Status = call.Status
            };

            _dal.Call.Update(updatedCall);
        }

        public void DeleteCall(int id)
        {
            _dal.Call.Delete(id);
        }

        public void AddCall(Call call)
        {
            var newCall = new DO.Call
            {
                Id = call.Id,
                Details = call.Details,
                Status = call.Status
            };

            _dal.Call.Create(newCall);
        }
    }
}
