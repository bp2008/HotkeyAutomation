import Vue from 'vue';
import VueRouter from 'vue-router';

import ClientLayout from 'appRoot/vues/ClientLayout.vue';
import PassThroughChild from 'appRoot/vues/common/PassThroughChild.vue';
import LogPage from 'appRoot/vues/LogPage.vue';
import HotkeyList from 'appRoot/vues/hotkeys/HotkeyList.vue';
import iTachList from 'appRoot/vues/itach/iTachList.vue';
import VeraList from 'appRoot/vues/vera/VeraList.vue';
import BroadLinkList from 'appRoot/vues/broadlink/BroadLinkList.vue';
import BroadLinkCommandList from 'appRoot/vues/broadlink/BroadLinkCommandList.vue';
import SystemConfiguration from 'appRoot/vues/system/SystemConfiguration.vue';

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
					},
					{
						path: 'broadlink', component: PassThroughChild,
						children: [
							{ path: '', component: BroadLinkList, name: 'broadlinks' }
						]
					},
					{
						path: 'system', component: PassThroughChild,
						children: [
							{ path: '', component: SystemConfiguration, name: 'system' }
						]
					},
					{
						path: 'broadlinkcmds/:controllerId', component: BroadLinkCommandList, name: 'broadlinkcmds', props: true
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