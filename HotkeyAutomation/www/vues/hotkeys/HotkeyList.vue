<template>
	<List componentName="HotkeyListItem" itemType="hotkey" apiKey="hotkey"></List>
</template>

<script>
	import List from 'appRoot/vues/common/List.vue';

	export default {
		components: { List },
		created()
		{
			this.$store.dispatch("CacheApiResponse", "broadlink_names");
			this.$store.dispatch("CacheApiResponse", "itach_names");
			this.$store.dispatch("CacheApiResponse", "vera_names");
			this.$store.dispatch("CacheApiResponse", "vera_command_list");
			this.$store.dispatch("CacheApiResponse", "broadlink_command_short_names");
			this.$store.dispatch("CacheApiResponse", "ir_command_short_names");
		}
	};
</script>