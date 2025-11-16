// using CardWar.Factories;
using System.Threading.Tasks;
using CardWar.Untils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// using CardWar.Views;

namespace CardWar_v2.SceneViews
{
    public class FightView : MonoBehaviour
    {
        [SerializeField] private Button _arenaBtn;
        [SerializeField] private Button _campaignBtn;

        void Start()
        {
            _campaignBtn.onClick.AddListener(async () => await SceneNavigator.Instance.ChangeScene(EScene.Campaign));
            _arenaBtn.onClick.AddListener(async () => await SceneNavigator.Instance.ChangeScene(EScene.Arena));
        }
    }
}