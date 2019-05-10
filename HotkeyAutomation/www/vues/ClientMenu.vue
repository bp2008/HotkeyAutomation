<template>
	<b-navbar toggleable="sm" type="dark" variant="primary" :sticky="true">

		<b-navbar-brand href="#" variant="faded">{{systemName}}</b-navbar-brand>

		<b-navbar-toggle target="nav_collapse"></b-navbar-toggle>

		<b-collapse is-nav id="nav_collapse">

			<b-navbar-nav>
				<b-nav-item :to="{ name: 'log' }">Log</b-nav-item>
				<b-nav-item :to="{ name: 'hotkeys' }">Hotkeys</b-nav-item>
				<b-nav-item :to="{ name: 'itachs' }">iTach Controllers</b-nav-item>
				<b-nav-item :to="{ name: 'veras' }">Vera Controllers</b-nav-item>
				<b-nav-item :to="{ name: 'broadlinks' }">BroadLink Controllers</b-nav-item>
			</b-navbar-nav>

		</b-collapse>
	</b-navbar>
</template>

<script>
	import ExecJSON from 'appRoot/api/api.js';
	export default {
		data: function ()
		{
			return {
				systemName: appContext.systemName
			};
		},
		computed:
		{
		},
		methods:
		{
		},
		created()
		{
		}
	};
</script>

<style scoped>
	.smallText
	{
		font-size: 0.7em;
	}
	/*nav
	{
		display: flex;
		flex-wrap: wrap;
		flex-direction: column;
		background-color: #808080;
		padding: 15px 30px;
	}

		nav *
		{
			margin: 0px 10px;
			font-size: 20px;
			color: #000000;
			text-decoration: none;
		}

	@media (min-width: 600px)
	{
		nav
		{
			flex-direction: row;
		}
	}*/
</style>