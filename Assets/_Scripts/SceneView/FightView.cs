// using CardWar.Factories;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

// using CardWar.Views;

namespace CardWar_v2.SceneViews
{
    public class FightView : MonoBehaviour
    {
        private string _campaignScene = "Campaign";
        private string _arenaScene = "Arena";

        public async void NavToCampaign()
        {
            await SceneManager.LoadSceneAsync(_campaignScene);
        }

        public async void NavToArena()
        {
            await SceneManager.LoadSceneAsync(_arenaScene);
        }
    }
}