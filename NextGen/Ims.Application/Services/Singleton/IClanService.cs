using Pos.Model.Model.Orther;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pos.Application.Services.Singleton
{
    public interface IClanService
    {
        Task<IEnumerable<Clan>> ReadAllAsync();
    }

    public class ClanService : IClanService
    {
        public async Task<IEnumerable<Clan>> ReadAllAsync()
        {
            await Task.Delay(3000); // Simulate a delay of 3 seconds.
            // Generate a list of integers.
            List<Clan> numbers = new List<Clan>();
            for (int i = 1; i <= 10; i++)
            {
                Clan tmp = new Clan();
                tmp.Name = "Name " + i.ToString();
                numbers.Add(tmp);
            }
            return numbers;
        }
    }
}
