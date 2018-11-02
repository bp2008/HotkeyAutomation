import Vue from 'vue';
import VueRouter from 'vue-router';

import ClientLayout from 'appRoot/vues/ClientLayout.vue';
import PassThroughChild from 'appRoot/vues/common/PassThroughChild.vue';
import LogPage from 'appRoot/vues/LogPage.vue';
import HotkeyList from 'appRoot/vues/hotkeys/HotkeyList.vue';
import iTachList from 'appRoot/vues/itach/iTachList.vue';
import VeraList from 'appRoot/vues/vera/VeraList.vue';

Vue.use(VueRouter);

export default function CreateRouter(store, basePath)
{
	const router = new VueRouter({
		mode: 'history',
		routes: [
			{
				path: basePath + '', component: ClientLayout,
				children: [
					{ path: '', redirect: 'log' },
					{
						path: 'log', component: PassThroughChild,
						children: [
							{ path: '', component: LogPage, name: 'log' }
						]
					},
					{ path: '', redirect: 'hotkeys' },
					{
						path: 'hotkeys', component: PassThroughChild,
						children: [
							{ path: '', component: HotkeyList, name: 'hotkeys' }
						]
					},
					{
						path: 'itach', component: PassThroughChild,
						children: [
							{ path: '', component: iTachList, name: 'itachs' }
						]
					},
					{
						path: 'vera', component: PassThroughChild,
						children: [
							{ path: '', component: VeraList, name: 'veras' }
						]
					}
				]
			}
		],
		$store: store
	});

	router.onError(function (error)
	{
		console.error("Error while routing", error);
		toaster.error('Routing Error', error);
	});

	router.beforeEach((to, from, next) =>
	{
		if (document)
			document.title = "HotkeyAutomation";

		next();
	});

	return router;
}