<template>
	<div class="systemConfigurationRoot">
		<h2>Download Configuration</h2>
		<p><a :href="DownloadConfigurationPath">Download Configuration (zip)</a></p>
		<h2>Upload Configuration</h2>
		<div v-if="uploadStatus">
			<p>{{uploadPercent}}%</p>
			<p>{{uploadStatus}}</p>
		</div>
		<template v-else>
			<p>
				<input type="file"
					   class="fileInput"
					   ref="fileInput"
					   accept=".zip"
					   @change="fileInputChangeCounter++" />
			</p>
			<p v-if="selectedFile">
				<input v-show="selectedFileIsZip" type="button" value="Upload Now" @click="uploadFile" />
				<span v-show="!selectedFileIsZip">Zip File Only!</span>
			</p>
		</template>
		<h2>Buzzer Configuration</h2>
		<p>This software can be configured so that upon keypress, sound is emitted from an active buzzer attached to a GPIO pin.  Only tested on Raspberry PI.</p>
		<p>If you set the GPIO number to a value greater than 0, we'll try to use a buzzer attached to that pin.</p>
		<div v-if="buzzerCfg">
			<div>
				<label>Buzzer GPIO Number: <input type="number" min="0" max="30" v-model="buzzerCfg.buzzerGpioNumber" /></label>
			</div>
			<div>
				<label><input type="checkbox" v-model="buzzerCfg.buzzerGpioOutputLowToBeep" /> Check this box if your buzzer is active when the GPIO output is "Low". Uncheck if "High".</label>
			</div>
			<div><input type="button" value="Save Buzzer Configuration" @click="saveBuzzerConfiguration()" /></div>
		</div>
		<div v-else-if="buzzerCfgError">Loading buzzer configuration...</div>
		<div v-else>Loading buzzer configuration...</div>
		<p><img :src="rpiPinoutImgUrl" style="width:640px;height:427px" /> <img :src="rpiPiezoBuzzerImgUrl" style="width:640px;height:444px" /></p>
	</div>
</template>

<script>
	import { ExecJSON } from 'appRoot/api/api.js';

	export default {
		components: {},
		data()
		{
			return {
				fileInputChangeCounter: 0,
				uploadStatus: null,
				uploadPercent: 0,
				rpiPinoutImgUrl: appContext.appPath + "images/rpi-40-pin.png",
				rpiPiezoBuzzerImgUrl: appContext.appPath + "images/rpi-piezo-buzzer.jpg",
				buzzerCfg: null,
				buzzerCfgError: null,
			};
		},
		async created()
		{
			try
			{
				this.buzzerCfg = (await ExecJSON({ cmd: "get_buzzer_config" })).data;
			}
			catch (ex)
			{
				this.buzzerCfgError = ex.message;
				toaster.error(ex);
			}
		},
		computed:
		{
			DownloadConfigurationPath()
			{
				return appContext.appPath + 'DownloadConfiguration';
			},
			selectedFile()
			{
				if (this.fileInputChangeCounter > 0)
					return this.$refs.fileInput.files[0];
				return null;
			},
			selectedFileIsZip()
			{
				if (this.selectedFile && this.selectedFile.name.match(/\.zip$/i))
					return true;
				return false;
			}
		},
		methods:
		{
			uploadFile()
			{
				this.uploadStatus = "Uploading…";
				this.selectedFile.arrayBuffer()
					.then(arrayBuffer =>
					{
						var data = new Uint8Array(arrayBuffer);

						let request = new XMLHttpRequest();
						request.open('POST', appContext.appPath + 'UploadConfiguration');

						request.upload.addEventListener('progress', e =>
						{
							this.uploadPercent = Math.round((e.loaded / e.total) * 100);
						});

						request.addEventListener('load', e =>
						{
							if (request.status === 200)
							{
								if (request.response === "1")
									this.uploadStatus = "Configuration was accepted. Server restart imminent.";
								else if (request.response === "0")
									this.uploadStatus = "Configuration was not accepted.";
								else
									this.uploadStatus = "Unexpected response from server: " + request.response;
							}
							else
							{
								this.uploadStatus = "Upload failed with code " + request.status + ". " + request.response;
							}
						});
						request.addEventListener("error", e =>
						{
							console.log(e);
							this.uploadStatus = "An error prevented the configuration from being uploaded. See developer console for more details.";
						});
						request.addEventListener("abort", e =>
						{
							console.log(e);
							this.uploadStatus = "The configuration upload was canceled.";
						});

						// send POST request to server
						request.send(data);
					});
			},
			async saveBuzzerConfiguration()
			{
				try
				{
					await ExecJSON({ cmd: "set_buzzer_config", data: this.buzzerCfg });
				}
				catch (ex)
				{
					toaster.error(ex);
				}
			}
		}
	};
</script>
<style scoped>
	.systemConfigurationRoot
	{
		margin: 10px;
	}

	.fileInput
	{
		border: 1px solid black;
		padding: 5px;
	}

		.fileInput:drop
		{
			background-color: #DDAA00;
		}

		.fileInput:drop(invalid active)
		{
			background-color: #FF0000;
		}
</style>