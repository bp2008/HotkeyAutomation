import ModalDialogComponent from 'appRoot/vues/common/dialogs/ModalDialog.vue';
const createModalDialog = create({ component: ModalDialogComponent, wrapper: 'dialogFade' });

import HotkeyListener from 'appRoot/vues/common/dialogs/HotkeyListener.vue';

export function ModalDialog(contentComponent, contentProps)
{
	return createModalDialog({ contentComponent, contentProps });
}

export function ModalHotkeyListener(hotkeyId)
{
	return ModalDialog(HotkeyListener, { hotkeyId });
}

/////////////////////////////////
// Modal Dialog Base           //
/////////////////////////////////
let allContainers = {};
export function RegisterModalDialogContainer(containerComponent)
{
	if (allContainers[containerComponent.name] !== containerComponent)
		allContainers[containerComponent.name] = containerComponent;
}
export function UnregisterModalDialogContainer(containerComponent)
{
	if (allContainers[containerComponent.name] === containerComponent)
	{
		delete allContainers[containerComponent.name];
	}
}
/**
 * Returns a function to create a dialog from the specified component, in the specified container.
 * @param {Object} param0 An object containing two properties. [component] should be a reference to a component (suggestion: load via "import"). [wrapper] should be a string name of a ModalDialogContainer element that has been added to the root vue component.
 * @returns {Function} Returns a function.  The function accepts as an argument an object defining the props to be passed to the created component.  The function returns a promise which resolves when the dialog is closed.
 */
function create({ component, wrapper })
{
	return props =>
	{
		return new Promise((resolve, reject) =>
		{
			let container = allContainers[wrapper];
			if (!container)
			{
				console.error('Could not find dialog container. <ModalDialogContainer name="' + wrapper + '" /> should be added somewhere in the project.');
				resolve(false);
				return;
			}
			container.CreateDialog(component, props, dialogResult =>
			{
				// Called upon dialog close
				resolve(dialogResult);
			});
		});
	};
}
export function CountOpenDialogs(condition)
{
	let total = 0;
	for (let key in allContainers)
		if (allContainers.hasOwnProperty(key))
		{
			let c = allContainers[key];
			if (c && c.components)
			{
				if (typeof condition === "function")
				{
					for (let i = 0; i < c.components.length; i++)
						if (condition(c.components[i]))
							total++;
				}
				else if (c.components.length)
					total += c.components.length;
			}
		}
	return total;
}