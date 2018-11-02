import Vue from 'vue';
import Vuex from 'vuex';
Vue.use(Vuex);
import createPersistedState from 'vuex-persistedstate';

import { ExecJSON } from 'appRoot/api/api.js';

export default function CreateStore()
{
	let loading = {};
	let retryLoad = {};
	return new Vuex.Store({
		strict: false, // TODO: Disable 'strict' for releases to improve performance
		plugins: [createPersistedState({ storage: window.sessionStorage })],
		state:
		{
			apiResponseCache: {}
		},
		mutations: // mutations must not be async
		{
			SetCachedResponse: (state, { cmd, names }) =>
			{
				Vue.set(state.apiResponseCache, cmd, names);
			}
		},
		actions: // actions can be async
		{
			/**
			 * Loads data from the specified API request.  Components wishing to use the names should create an appropriate computed property.
			 * @param {any} param0 Store reference
			* @param {String} cmd API Key, e.g. "vera_list"
			 */
			CacheApiResponse({ commit, dispatch, state, rootState, rootGetters }, cmd)
			{
				if (loading[cmd])
					return;
				loading[cmd] = true;
				ExecJSON({ cmd: cmd }).then(data =>
				{
					clearTimeout(retryLoad[cmd]);
					retryLoad[cmd] = null;
					commit("SetCachedResponse", { cmd: cmd, names: data.data });
				}
				).catch(err =>
				{
					console.error(err);
					if (!retryLoad[cmd])
					{
						toaster.Error("Data Loading Failed", "Unable to load " + cmd, 5000);
						retryLoad[cmd] = setTimeout(() =>
						{
							retryLoad[cmd] = null;
							dispatch("CacheApiResponse", cmd);
						}, 2500);
					}
				}
				).finally(() =>
				{
					loading[cmd] = false;
				});
			}
		},
		getters:
		{
			GetCachedResponse: state => cmd =>
			{
				return state.apiResponseCache[cmd];
			}
		}
	});
}