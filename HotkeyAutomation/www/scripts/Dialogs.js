import HotkeyListener from 'appRoot/vues/common/controls/HotkeyListener.vue';
import { create } from 'vue-modal-dialogs';
const hotkeyListenerDialog = create({ component: HotkeyListener, wrapper: 'dialogFade' });

export function ModalHotkeyListener(hotkeyId)
{
	let args = { hotkeyId: hotkeyId };
	return hotkeyListenerDialog(args);
}