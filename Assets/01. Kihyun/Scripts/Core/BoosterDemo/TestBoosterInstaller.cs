//using UnityEngine;

//// 부스터 테스트용
//// 파츠 시스템 추가 후 삭제 예정
//public class TestBoosterInstaller : MonoBehaviour
//{
//    [Header("Player Components")]
//    [SerializeField] private PlayerLocomotionMotor _locomotionMotor;
//    [SerializeField] private CoreEnergyController _corePower;
//    [SerializeField] private PlayerInputHandler _inputHandler;

//    [Header("Test Booster")]
//    [SerializeField] private BoosterPart _boosterPart;

//    private void Start()
//    {
//        if (_locomotionMotor == null)
//            _locomotionMotor = GetComponent<PlayerLocomotionMotor>();

//        if (_corePower == null)
//            _corePower = GetComponent<CoreEnergyController>();

//        if (_inputHandler == null)
//            _inputHandler = GetComponent<PlayerInputHandler>();

//        if (_boosterPart != null)
//        {
//            _boosterPart.Initialize(
//                _locomotionMotor,
//                _corePower,
//                _inputHandler
//            );
//        }
//    }
//}