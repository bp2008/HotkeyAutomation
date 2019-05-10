<template>
	<NamedListItem ref="item" :item="item" apiKey="broadlink" itemType="BroadLink RM" :includePort="false" v-on="$listeners" />
</template>

<script>
	import NamedListItem from 'appRoot/vues/common/NamedListItem.vue';

	export default {
		components: { NamedListItem },
		props:
		{
			item: {
				type: Object,
				required: true
			}
		},
		methods:
		{
			resetState()
			{
				if (this.$refs.item.resetState)
					this.$refs.item.resetState();
			}
		}
	};
</script>