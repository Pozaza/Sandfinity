using System;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace UnityEngine.UI {
	[AddComponentMenu("UI/CustomButton", 30)]
	public class CustomButton : Selectable, IPointerClickHandler, ISubmitHandler {
		[Serializable]
		public class ButtonClickedEvent : UnityEvent { }

		[FormerlySerializedAs("onClick")]
		[SerializeField]
		ButtonClickedEvent m_OnClick = new();

		public ButtonClickedEvent GetonClick() { return m_OnClick; }
		public void SetonClick(ButtonClickedEvent value) { m_OnClick = value; }

		void Press() {
			if (!IsActive() || !IsInteractable())
				return;

			UISystemProfilerApi.AddMarker("Button.onClick", this);
			m_OnClick.Invoke();
		}

		public virtual void OnPointerClick(PointerEventData eventData) {
			if (eventData.button != PointerEventData.InputButton.Left)
				return;

			Press();
		}

		public virtual void OnSubmit(BaseEventData eventData) {
			Press();

			if (!IsActive() || !IsInteractable())
				return;

			DoStateTransition(SelectionState.Pressed, false);
			StartCoroutine(OnFinishSubmit());
		}

		IEnumerator OnFinishSubmit() {
			var fadeTime = colors.fadeDuration;
			var elapsedTime = 0f;

			while (elapsedTime < fadeTime) {
				elapsedTime += Time.unscaledDeltaTime;
				yield return null;
			}

			DoStateTransition(currentSelectionState, false);
		}
		public void SendItemRequest() {
			MouseLook.Instance.GetRequest(gameObject.name);
		}
	}
}
