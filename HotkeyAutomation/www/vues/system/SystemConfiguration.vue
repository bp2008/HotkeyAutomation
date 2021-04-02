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
	</div>
</template>

<script>
	export default {
		components: {},
		data()
		{
			return {
				fileInputChangeCounter: 0,
				uploadStatus: null,
				uploadPercent: 0
			};
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