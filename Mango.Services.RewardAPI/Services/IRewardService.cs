
using Mango.Services.RewardAPI.Message;

namespace Mango.Services.RewardAPI
{
    public interface IRewardService
    {
        Task UpdateRewards(RewardsMessage rewardsMessage);
    }
}
