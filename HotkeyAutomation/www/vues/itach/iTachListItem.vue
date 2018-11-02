<template>
	<NamedListItem ref="item" :item="item" apiKey="itach" itemType="iTach" :includePort="true" portPlaceholder="4998" v-on="$listeners" />
</template>

<script>
	import { ExecJSON } from 'appRoot/api/api.js';
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