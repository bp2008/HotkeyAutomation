<template>
	<div class="dialogRoot" @keyup.esc.prevent="defaultClose" @mousedown="overlayMouseDown" @click="overlayClick">
		<div class="FocusCatcher" tabindex="0" @focus="FocusCatcher(false)"></div>
		<div class="dialogContent" @mousedown="contentMouseDown" @click="contentClick">
			<component v-bind:is="contentComponent"
					   v-bind="contentProps"
					   ref="contentComponent"
					   @close="closeRequested" />
		</div>
		<div class="FocusCatcher" tabindex="0" @focus="FocusCatcher(true)"></div>
	</div>
</template>

<script>
	// All modal dialogs should use this component.
	export default {
		props:
		{
			contentComponent: {
				type: Object,
				required: true
			},
			contentProps: null
		},
		data()
		{
			return {
				oldFocus: null,
				lastMouseDownWasOnOverlay: false
			};
		},
		methods:
		{
			contentMouseDown(e)
			{
				e.stopPropagation();
				this.lastMouseDownWasOnOverlay = false;
			},
			contentClick(e)
			{
				e.stopPropagation();
			},
			overlayMouseDown(e)
			{
				this.lastMouseDownWasOnOverlay = true;
			},
			overlayClick(e)
			{
				if (this.lastMouseDownWasOnOverlay)
					this.defaultClose();
			},
			defaultClose()
			{
				if (this.$refs.contentComponent && typeof this.$refs.contentComponent.DefaultClose === "function")
					this.$refs.contentComponent.DefaultClose();
				else
					this.$emit("close", false);
			},
			closeRequested(args)
			{
				if (typeof args === "undefined")
					this.$emit("close", false);
				else
					this.$emit("close", args);
			},
			LostFocus()
			{
				this.SetFocus();
			},
			SetFocus()
			{
				if (!this.$refs.contentComponent)
					return;
				if (typeof this.$refs.contentComponent.SetFocus === "function")
					this.$refs.contentComponent.SetFocus();
				else
					this.FocusCatcher(true);
			},
			FocusCatcher(focusFirstItem)
			{
				let focusable = this.$refs.contentComponent.$el.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])')
				if (focusable.length > 0)
				{
					if (focusFirstItem)
						focusable[0].focus();
					else
						focusable[focusable.length - 1].focus();
				}
			}
		},
		mounted()
		{
			this.$data.oldFocus = document.activeElement;
			this.SetFocus();
			let layoutRoot = document.getElementById("layoutRoot");
			if (layoutRoot)
				layoutRoot.addEventListener("focusin", this.LostFocus, true);
		},
		beforeDestroy()
		{
			let layoutRoot = document.getElementById("layoutRoot");
			if (layoutRoot)
				layoutRoot.removeEventListener("focusin", this.LostFocus, true);
			if (this.$data.oldFocus)
				this.$data.oldFocus.focus();
		}
	}
</script>

<style scoped>
	.dialogRoot
	{
		position: fixed;
		z-index: 10000;
		top: 0px;
		left: 0px;
		width: 100%;
		height: 100%;
		background-color: rgba(0,0,0,0.15);
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
	}

	.dialogContent
	{
		max-width: 90vw;
		max-height: 90vh;
		border: 1px solid #888888;
		border-radius: 7px;
		box-shadow: 0px 0px 3px rgba(0,0,0,0.5);
		background-color: #FFFFFF;
		font-size: 16px;
		padding: 5px;
	}
</style>