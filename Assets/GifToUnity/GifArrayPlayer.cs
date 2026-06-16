using GifImporter;
using UnityEngine;
using UnityEngine.UI;

namespace GifImporter
{
    [ExecuteAlways]
    [RequireComponent(typeof(Image))] // UI Canvas에서 쓸 거니까 필수 장착!
    public class GifArrayPlayer : MonoBehaviour
    {
        // 🌟 [핵심 변경] 단일 에셋이 아니라, 인스펙터에서 마우스로 여러 개 등록할 수 있는 GIF 배열 개설!
        [Header("[ 가이드 페이지별 GIF 에셋 배열들 ]")]
        [SerializeField] private Gif[] _gifArray;

        private int _currentFrameIndex;
        private float _flipTime;
        private Gif _activeGif;

        private int _currentGifIndex = 0;

        private void OnEnable()
        {
            // 가이드 패널이 켜지면 무조건 '첫 번째(0번)' GIF 에셋으로 재생 초기화
            ChangeGifByIndex(0);
        }

        // 🚀 외부(GuideUIController 등)에서 "야, 다음 가이드 페이지 GIF로 바꿔!" 하고 호출할 마법의 함수
        public void ChangeGifByIndex(int index)
        {
            if (_gifArray == null || _gifArray.Length == 0) return;

            // 예외 처리: 배열 범위를 벗어나지 않게 방어
            if (index < 0 || index >= _gifArray.Length)
            {
                Debug.LogWarning($"[GifArrayPlayer] 요청한 인덱스 {index}가 GIF 배열 범위를 벗어났습니다.");
                return;
            }

            // 현재 가동 중인 인덱스 저장 및 Gif 에셋 교체 교환!
            _currentGifIndex = index;
            _activeGif = _gifArray[_currentGifIndex];

            // 프레임 인덱스 및 타이머 산뜻하게 리셋
            _currentFrameIndex = 0;
            _flipTime = 0f;

            // 첫 프레임 즉시 갱신 렌더링
            UpdateFrameRender();
        }

        private void Update()
        {
            if (_activeGif == null) return;
            var frames = _activeGif.Frames;
            if (frames == null || frames.Count == 0) return;

            int nextFrame = _currentFrameIndex;

            // 게임 플레이 도중이고 프레임 전환 시간이 지났다면 다음 프레임 인덱스 증가
            if (Application.isPlaying && _flipTime < Time.time)
            {
                nextFrame++;
            }

            // 마지막 프레임 도달 시 무한루프 처리
            if (nextFrame > frames.Count - 1)
            {
                nextFrame %= frames.Count;
            }

            // 프레임이 바뀔 때만 실제 이미지 스프라이트를 새로 갱신!
            if (nextFrame != _currentFrameIndex)
            {
                _currentFrameIndex = nextFrame;
                UpdateFrameRender();
            }
        }

        // 🖼️ 실제 UI Image 컴포넌트에 스프라이트를 이식시켜주는 연산 함수
        private void UpdateFrameRender()
        {
            if (_activeGif == null) return;
            var frames = _activeGif.Frames;
            if (frames == null || frames.Count == 0 || _currentFrameIndex >= frames.Count) return;

            var currentFrame = frames[_currentFrameIndex];

            if (TryGetComponent<Image>(out var uiImage))
            {
                // 밀리초(Ms)를 초(Seconds) 단위로 변환해서 딜레이 타임 세팅
                _flipTime = Time.time + currentFrame.DelayInMs * 0.001f;

                // UI 이미지 스프라이트 탁 교체!
                uiImage.sprite = currentFrame.Sprite;
            }
        }
    }
}