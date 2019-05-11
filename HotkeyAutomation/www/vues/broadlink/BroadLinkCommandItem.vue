<template>
	<NamedListItem ref="item" :item="item" apiKey="broadlinkcmd" itemType="BroadLinkCommand" v-on="$listeners" @learnCodes="learnCodes"/>
</template>

<script>
	import NamedListItem from 'appRoot/vues/common/NamedListItem.vue';
	import { ModalBroadLinkLearnCodesListener } from 'appRoot/scripts/Dialog.js';

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
			},
			learnCodes()
			{
				ModalBroadLinkLearnCodesListener(parseInt(this.$route.params.controllerId), this.item.id).then(result =>
				{
					if (result)
					{
						this.item.type = result.type;
						this.item.codes = result.codes;
					}
				});
			}
		}
	};
</script>