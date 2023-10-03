using GalaSoft.MvvmLight;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using dbTransferUser_UserControl.ResponseObjects.Projects;
using dbTransferUser_UserControl.ResponseObjects.Background;
using dbTransferUser_UserControl.ResponseObjects.Company;
using dbTransferUser_UserControl.ResponseObjects.FinalMp4;
using dbTransferUser_UserControl.ResponseObjects.Media;
using dbTransferUser_UserControl.ResponseObjects.Screen;
using dbTransferUser_UserControl.ResponseObjects.App;
using dbTransferUser_UserControl.ResponseObjects;
using dbTransferUser_UserControl.ResponseObjects.VideoEvent;
using System.Net.Http;
using System.Windows;
using System.Text;
using System.IO;
using System.Runtime.CompilerServices;

namespace VideoCreator.Auth
{
    internal class AuthAPIViewModel : ViewModelBase
    {
        private readonly DateTime MODIFIEDDATE;

        #region === Properties ===

        private readonly AuthApiClientHelper _apiClientHelper;

        private string _projectString;
        public string ProjectString
        {
            get => _projectString;
            set => Set(nameof(ProjectString), ref _projectString, value);
        }

        //private List<ProjectModel> _allProjects;
        //public List<ProjectModel> AllProjects
        //{
        //    get => _allProjects;
        //    set => Set(nameof(AllProjects), ref _allProjects, value);
        //}


        #endregion

        public AuthAPIViewModel()
        {
            _apiClientHelper = new AuthApiClientHelper();
            MODIFIEDDATE = new DateTime(2023, 01, 01);// DateTime.UtcNow.AddDays(-30);
        }

        public bool Isbusy()
        {
            return _apiClientHelper.IsBusy;
        }

        public string GetError()
        {
            return _apiClientHelper.ErrorMessage;
        }

        public string GetToken()
        {
            return _apiClientHelper.TokenNumber;
        }

        public string GetAccessKey()
        {
            return _apiClientHelper.AccessKey;
        }
        public async Task ExecuteLoginAsync()
        {
            await _apiClientHelper.ExecuteLoginAsync();
        }

        public async Task ExecuteLogoutAsync()
        {
            await _apiClientHelper.ExecuteLogoutAsync();
        }


        
        #region == Project API Calls ==

        public async Task<List<ProjectModel>> GetProjectsData(DateTime? modifiedDate = null, ProjectStatusEnum status = 0)
        {
            var url = $"api/connect/project/available";
            if (status > 0)
                url += $"?projstatus={(int)status}";
            if (modifiedDate.HasValue)
                url += $"?modifydate={modifiedDate:yyyy-MM-dd HH:mm:ss}";

            var result = await _apiClientHelper.Get<ParentDataList<ProjectModel>>(url);
            return result?.Data?.Count > 0 ? result.Data : null;
        }

        public async Task<ParentData<ProjectAcceptRejectModel>> AcceptOrRejectProject(int project_id, bool accept_flag)
        {
            var url = $"api/connect/project/accept-reject";
            var parameters = new Dictionary<string, string>
            {
                { "project_id", project_id.ToString() },
                { "accept_reject", accept_flag ? "1" : "0" }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<ProjectAcceptRejectModel>>(url, payload);
            return result != null ? result : null;
        }
        
        public async Task CreateProject(string project_name, int fk_project_section, int fk_project_projstatus, int project_version, string project_comments)
        {
            var url = $"api/connect/project";
            var parameters = new Dictionary<string, string>
            {
                { "project_name", project_name.ToString() },
                { "fk_project_section", fk_project_section.ToString() },
                { "fk_project_projstatus", fk_project_projstatus.ToString() },
                { "project_version", project_version.ToString() },
                { "project_comments", project_comments.ToString() },
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<ProjectModel>>(url, payload);
            await GetProjectsData();
        }

        public async Task UpdateProject(int ProjectId, string project_name, int fk_project_section, int fk_project_projstatus, int project_version, string project_comments)
        {
            var url = $"api/connect/project/{ProjectId}";
            var parameters = new Dictionary<string, string>
            {
                { "project_name", project_name.ToString() },
                { "fk_project_section", fk_project_section.ToString() },
                { "fk_project_projstatus", fk_project_projstatus.ToString() },
                { "project_version", project_version.ToString() },
                { "project_comments", project_comments.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<ProjectModel>>(url, payload);
            await GetProjectsData();
        }

        public async Task PatchProject(int projectId, int project_version, string project_comments)
        {
            var url = $"api/connect/project/{projectId}";
            var parameters = new Dictionary<string, string>
            {
                { "project_version", project_version.ToString() },
                { "project_comments", project_comments.ToString() },
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<ProjectModel>>(url, payload);
            await GetProjectsData();
        }

        public async Task GetProjectCount()
        {
            var url = $"api/connect/project/count";
            var result = await _apiClientHelper.Get<ProjectCountModel>(url);
            if (result != null && result.Available <= -1)
            {
                MessageBox.Show("Some Error occured !!!", "GetProjectCount", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show($"Available number of projects: [ {result.Available} ]", "GetProjectCount", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public async Task GetOwnershipOfProjects(string ProjectIds)
        {
            var url = $@"api/connect/project/owner?project_id={ProjectIds}";
            var result = await _apiClientHelper.Get<Dictionary<string, bool>>(url);
            if (result == null)
                MessageBox.Show("Some Error occured !!!", "GetOwnershipOfProjects", MessageBoxButton.OK, MessageBoxImage.Error);
            else
            {
                var builder = new StringBuilder();
                builder.Append("Project Id" + "\t   \t" + "Owner" + "\n");
                builder.Append("==========" + "\t   \t" + "=====" + "\n");
                foreach (var keyPair in result)
                {
                    builder.Append(keyPair.Key + "\t\t : \t" + keyPair.Value + "\n");
                }
                MessageBox.Show(builder.ToString(), "Projects ownership List", MessageBoxButton.OK, MessageBoxImage.Information);
            }

        }

        public async Task IsAssignedToAnyProject()
        {
            var url = "api/connect/project/assigned";
            var result = await _apiClientHelper.Get<ProjectAssignModel>(url);
            if (result != null && result.Assigned == false)
            {
                MessageBox.Show($"user not assigned to any project", "Project Assigned", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Assigned to any projects: [" + result.ToString() + "]", "Projects Assigned", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        #endregion

        #region == Master table data ==
        public async Task<AppModel> GetAllApp()
        {
            var url = "api/connect/app";
            var result = await _apiClientHelper.Get<AppModel>(url);
            if (result != null)
                return result;
            else
                MessageBox.Show($"No data found", "Synchronising Apps Data", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        public async Task<List<MediaModel>> GetAllMedia()
        {
            var url = "api/connect/media";
            var result = await _apiClientHelper.Get<ParentDataList<MediaModel>>(url);
            if (result?.Data?.Count > 0)
                return result.Data;
            else
                MessageBox.Show($"No data Found", "Synchronising Media Data", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        public async Task<List<ScreenModel>> GetAllScreens()
        {
            var url = "api/connect/screen";
            var result = await _apiClientHelper.Get<ParentDataList<ScreenModel>>(url);
            if (result?.Data?.Count > 0)
                return result.Data;
            else
                MessageBox.Show($"No data Found", "Synchronising screen Data", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        public async Task<List<CompanyModel>> GetAllCompany()
        {
            var url = $"api/connect/company";

            var result = await _apiClientHelper.Get<List<CompanyModel>>(url);
            if (result != null)
                return result;
            else
                MessageBox.Show($"No Company Found", "Synchronising company Data", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        public async Task<List<BackgroundModel>> GetAllBackground()
        {
            var url = $"api/connect/background";
            var result = await _apiClientHelper.Get<ParentData<List<BackgroundModel>>>(url);
            if (result != null)
                return result?.Data;
            else
                MessageBox.Show($"No Background Found", "Synchronising Background Data", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }

        public async Task<string> DownloadBackground(int backgroundId, string extension)
        {
            var url = $"api/connect/background/download/{backgroundId}";
            var filename = $"background_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            var filepath = $@"{Directory.GetCurrentDirectory()}\\{filename}";
            return filepath;
        }


        #endregion
        /*
        #region == Company ==
        public async Task CreateCompany(string company_name)
        {
            var url = $"api/connect/company";
            var parameters = new Dictionary<string, string>
            {
                { "company_name", company_name }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<CompanyModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateCompany", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "CreateCompany", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateCompany(int companyId, string company_name)
        {
            var url = $"api/connect/company/{companyId}";
            var parameters = new Dictionary<string, string>
            {
                { "company_name", company_name }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<CompanyModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateCompany", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateCompany", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == Background ==

        
        //public async Task ListBackgroundById(int backgroundId)
        //{
        //    //var url = $"api/connect/background/download/{backgroundId}";
        //    //var result = await _apiClientHelper.Get<ParentData<BackgroundModel>>(url);
        //    //if (result != null)
        //    //{
        //    //    MessageBox.Show(result.Data.ToString(), $"ListBackgroundById - For {backgroundId}", MessageBoxButton.OK, MessageBoxImage.Information);
        //    //}
        //    //else
        //    //{
        //    //    MessageBox.Show($"No Background Found", "ListBackgroundById", MessageBoxButton.OK, MessageBoxImage.Error);
        //    //}
        //}

        public async Task CreateBackground(int fk_backgrounds_company, int backgrounds_active, string backgrounds_media)
        {
            var url = $"api/connect/background";
            var multipart = new MultipartFormDataContent();
            // FK
            var requestbodyContent = new StringContent(fk_backgrounds_company.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"fk_backgrounds_company\"");
            multipart.Add(requestbodyContent);

            // FK
            requestbodyContent = new StringContent(backgrounds_active.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"backgrounds_active\"");
            multipart.Add(requestbodyContent);

            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{backgrounds_media}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"backgrounds_media\"; filename=\"{backgrounds_media}\"");
            multipart.Add(fileStreamContent);

            var result = await _apiClientHelper.Create<ParentData<BackgroundModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateBackground", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }

        public async Task UpdateBackground(int backgroundId, int fk_backgrounds_company, int backgrounds_active)
        {
            var url = $"api/connect/background/{backgroundId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_backgrounds_company", fk_backgrounds_company.ToString() },
                { "backgrounds_active", backgrounds_active.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<CompanyModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateBackground", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateBackground", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task PatchBackground(int projectId, int videoeventId, int fk_videoevent_media, string videoevent_start, int videoevent_duration)
        {
            var url = $"api/connect/project/{projectId}/videoevent/{videoeventId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_videoevent_media", fk_videoevent_media.ToString() },
                { "videoevent_start", videoevent_start },
                { "videoevent_duration", videoevent_duration.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<VideoEventModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task DownloadBackground(int backgroundId, string extension)
        {
            var url = $"api/connect/background/download/{backgroundId}";
            var filename = $"background_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            MessageBox.Show($@"File Downloaded to path - {Directory.GetCurrentDirectory()}\\{filename}", "DownloadBackground", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region == Video Event ==

        public async Task ListAllVideoEvent(DateTime? modifiedDate = null)
        {
            var url = $"api/connect/project/2/videoevent";
            if (modifiedDate.HasValue)
                url += $"?modifydate={modifiedDate:yyyy-MM-dd HH:mm:ss}";

            var result = await _apiClientHelper.Get<ParentDataList<VideoEventModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total Record found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"List All Records - From {modifiedDate}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "ListAllVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task ListVideoEventById(int projectId, int videoEventId)
        {
            var url = $"api/connect/project/{projectId}/videoevent/{videoEventId}";
            var result = await _apiClientHelper.Get<ParentData<VideoEventModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"ListVideoEventById - For {videoEventId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "ListVideoEventById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateVideoEvent(int projectId, int fk_videoevent_media, int videoevent_track, string videoevent_start, int videoevent_duration)
        {
            var url = $"api/connect/project/{projectId}/videoevent";
            var parameters = new Dictionary<string, string>
            {
                { "fk_videoevent_media", fk_videoevent_media.ToString() },
                { "videoevent_track", videoevent_track.ToString() },
                { "videoevent_start", videoevent_start },
                { "videoevent_duration", videoevent_duration.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<VideoEventModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "CreateVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateVideoEvent(int projectId, int videoeventId, int fk_videoevent_media, int videoevent_track, string videoevent_start, int videoevent_duration)
        {
            var url = $"api/connect/project/{projectId}/videoevent/{videoeventId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_videoevent_media", fk_videoevent_media.ToString() },
                { "videoevent_track", videoevent_track.ToString() },
                { "videoevent_start", videoevent_start },
                { "videoevent_duration", videoevent_duration.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<VideoEventModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task PatchVideoEvent(int projectId, int videoeventId, int fk_videoevent_media, string videoevent_start, int videoevent_duration)
        {
            var url = $"api/connect/project/{projectId}/videoevent/{videoeventId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_videoevent_media", fk_videoevent_media.ToString() },
                { "videoevent_start", videoevent_start },
                { "videoevent_duration", videoevent_duration.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<VideoEventModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchVideoEvent", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchVideoEvent", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == Audio ==

        public async Task ListAllAudio(int videoEventId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/audio";

            var result = await _apiClientHelper.Get<ParentDataList<AudioModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total Audio found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"List All Audio for Video Event - {videoEventId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Audio Found", "ListAllAudio", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GetAudioById(int videoEventId, int audioId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/audio/{audioId}";

            var result = await _apiClientHelper.Get<ParentData<AudioModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"GetAudioById - For {videoEventId} videoevent in audio {audioId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "GetAudioById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateAudio(int fk_audio_videoevent, string filename)
        {
            var url = $"api/connect/videoevent/{fk_audio_videoevent}/audio";

            var multipart = new MultipartFormDataContent();
            // FK
            var requestbodyContent = new StringContent(fk_audio_videoevent.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"fk_audio_videoevent\"");
            multipart.Add(requestbodyContent);

            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{filename}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"audio_media\"; filename=\"{filename}\"");
            multipart.Add(fileStreamContent);

            var result = await _apiClientHelper.Create<ParentData<AudioModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateAudio", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }

        public async Task UpdateAudio(int videoeventId, int audioId, string filename)
        {
            var url = $"api/connect/videoevent/{videoeventId}/audio/{audioId}";

            var multipart = new MultipartFormDataContent();

            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{filename}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"audio_media\"; filename=\"{filename}\"");
            multipart.Add(fileStreamContent);

            //var result = await _apiClientHelper.Update<ParentData<AudioModel>>(url, multipart);
            var result = await _apiClientHelper.Create<ParentData<AudioModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateAudio", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }


        public async Task DownloadAudioBinary(int audioId, string extension)
        {
            var url = $"api/connect/download/audio/{audioId}";
            var filename = $"audio_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            MessageBox.Show($@"File Downloaded to path - {Directory.GetCurrentDirectory()}\\{filename}", "DownloadAudioBinary", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        #endregion

        #region == VideoSegments ==

        public async Task ListAllVideoSegments(int videoEventId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/videosegment";

            var result = await _apiClientHelper.Get<ParentDataList<VideoSegmentModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total VideoSegments found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"ListAllVideoSegments - {videoEventId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Video Segment Found", "ListAllVideoSegments", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GetVideoSegmentById(int videoEventId, int videosegmentId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/videosegment/{videosegmentId}";

            var result = await _apiClientHelper.Get<ParentData<VideoSegmentModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"GetVideoSegmentById - For {videoEventId} videoevent in videosegment {videosegmentId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "GetVideoSegmentById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateVideoSegmentBinary(int videoeventId, string videosegment_media)
        {
            var url = $"api/connect/videoevent/{videoeventId}/videosegment";

            var multipart = new MultipartFormDataContent();


            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{videosegment_media}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"videosegment_media\"; filename=\"{videosegment_media}\"");
            multipart.Add(fileStreamContent);

            var result = await _apiClientHelper.Create<ParentData<VideoSegmentModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateVideoSegment", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }

        public async Task UpdateVideoSegmentBinary(int videoeventId, int videosegmentId, string videosegment_media)
        {
            var url = $"api/connect/videoevent/{videoeventId}/videosegment/{videosegmentId}";

            var multipart = new MultipartFormDataContent();

            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{videosegment_media}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"videosegment_media\"; filename=\"{videosegment_media}\"");
            multipart.Add(fileStreamContent);

            //var result = await _apiClientHelper.Update<ParentData<AudioModel>>(url, multipart);
            var result = await _apiClientHelper.Create<ParentData<VideoSegmentModel>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "UpdateVideoSegment", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
        }

        public async Task DownloadBinaryVideoSegment(int videosegmentId, string extension)
        {
            var url = $"api/connect/download/videosegment/{videosegmentId}";
            var filename = $"videosegment_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            MessageBox.Show($@"File Downloaded to path - {Directory.GetCurrentDirectory()}\\{filename}", "DownloadBackground", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public async Task PatchVideoSegment(int fk_videosegment_videoevent, int videosegmentId)
        {
            var url = $"api/connect/videoevent/{fk_videosegment_videoevent}/videosegment/{videosegmentId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_videosegment_videoevent", fk_videosegment_videoevent.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<VideoSegmentModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchVideoSegment", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchVideoSegment", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == Design ==

        public async Task ListAllDesigns(int videoevent, DateTime? modifiedDate = null)
        {
            var url = $"api/connect/videoevent/{videoevent}/design";
            if (modifiedDate.HasValue)
                url += $"?modifydate={modifiedDate:yyyy-MM-dd HH:mm:ss}";

            var result = await _apiClientHelper.Get<ParentDataList<DesignModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total Record found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"List All Record - From {modifiedDate}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "ListAllDesigns", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task ListDesignById(int videoEventId, int designId)
        {
            var url = $"api/connect/videoevent/{videoEventId}/design/{designId}";
            var result = await _apiClientHelper.Get<ParentData<DesignModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"ListDesignById - For {designId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "ListDesignById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateDesign(int videoeventId, int fk_design_screen, string design_xml)
        {
            var url = $"api/connect/videoevent/{videoeventId}/design";
            var parameters = new Dictionary<string, string>
            {
                { "fk_design_screen", fk_design_screen.ToString() },
                { "design_xml", design_xml }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<DesignModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateDesign", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "CreateDesign", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateDesign(int videoeventId, int designId, int fk_design_videoevent, int fk_design_screen, string design_xml)
        {
            var url = $"api/connect/videoevent/{videoeventId}/design/{designId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_design_screen", fk_design_screen.ToString() },
                { "fk_design_videoevent", fk_design_videoevent.ToString() },
                { "design_xml", design_xml }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<DesignModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateDesign", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateDesign", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task PatchDesign(int videoeventId, int designId, int fk_design_screen, string design_xml)
        {
            var url = $"api/connect/videoevent/{videoeventId}/design/{designId}";
            var parameters = new Dictionary<string, string>
            {
                { "fk_design_screen", fk_design_screen.ToString() },
                { "design_xml", design_xml }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<DesignModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchDesign", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchDesign", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == Notes ==

        public async Task ListAllNotes(int videoeventId)
        {
            var url = $"api/connect/videoevent/{videoeventId}/notes";

            var result = await _apiClientHelper.Get<ParentDataList<NotesModel>>(url);
            if (result != null)
            {
                var builder = new StringBuilder();
                builder.Append($"Total Notes found - {result.Data.Count}" + "\n");
                foreach (var keyPair in result.Data)
                    builder.Append(keyPair.ToString() + "\n" + "\n" + "\n" + "\n");
                MessageBox.Show(builder.ToString(), $"List All Notes - For videoevent {videoeventId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "ListAllNotes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GetNotesById(int videoeventId, int notesId)
        {
            var url = $"api/connect/videoevent/{videoeventId}/notes/{notesId}";
            var result = await _apiClientHelper.Get<ParentData<NotesModel>>(url);
            if (result != null)
            {
                MessageBox.Show(result.Data.ToString(), $"GetNotesById - For {notesId}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "GetNotesById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateNotes(int fk_notes_videoevent, string notes_line, int notes_wordcount, int notes_index)
        {
            var url = $"api/connect/videoevent/{fk_notes_videoevent}/notes";
            var parameters = new Dictionary<string, string>
            {
                { "fk_notes_videoevent", fk_notes_videoevent.ToString() },
                { "notes_line", notes_line.ToString() },
                { "notes_wordcount", notes_wordcount.ToString() },
                { "notes_index", notes_index.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Create<ParentData<NotesModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateNotes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "CreateNotes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task UpdateNotes(int notesId, int fk_notes_videoevent, string notes_line, int notes_wordcount, int notes_index)
        {
            var url = $"api/connect/videoevent/{fk_notes_videoevent}/notes/{notesId}";
            var parameters = new Dictionary<string, string>
            {
                { "notes_wordcount", notes_wordcount.ToString() },
                { "notes_line", notes_line },
                { "notes_index", notes_index.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Update<ParentData<NotesModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "UpdateNotes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "UpdateNotes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task PatchNotes(int notesId, int fk_notes_videoevent, string notes_line)
        {
            var url = $"api/connect/videoevent/{fk_notes_videoevent}/notes/{notesId}";
            var parameters = new Dictionary<string, string>
            {
                { "notes_line", notes_line.ToString() }
            };
            var payload = new FormUrlEncodedContent(parameters);

            var result = await _apiClientHelper.Patch<ParentData<NotesModel>>(url, payload);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with updated data as - {result.Data}", "PatchNotes", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result?.Message, "PatchNotes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region == FINALMP4 ==

        public async Task GetFinalMp4ById(int projectId, int finalmp4Id)
        {
            var url = $"api/connect/mp4/{projectId}/video/{finalmp4Id}";
            var result = await _apiClientHelper.Get<FinalMp4Model>(url);
            if (result != null)
            {
                MessageBox.Show(result.ToString(), $"GetFinalMp4ById - For {finalmp4Id}", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"No Record Found", "GetFinalMp4ById", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CreateFinalMp4(int projectId, int finalmp4_version, string finalmp4_comments, string finalmp4_mediamp4)
        {
            var url = $"api/connect/mp4/{projectId}/video";

            var multipart = new MultipartFormDataContent();


            // FK
            var requestbodyContent = new StringContent(finalmp4_version.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"finalmp4_version\"");
            multipart.Add(requestbodyContent);


            // Comments
            requestbodyContent = new StringContent(finalmp4_comments.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"finalmp4_comments\"");
            multipart.Add(requestbodyContent);


            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{finalmp4_mediamp4}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"finalmp4_mediamp4\"; filename=\"{finalmp4_mediamp4}\"");
            multipart.Add(fileStreamContent);

            var result = await _apiClientHelper.Create<ParentData<FinalMp4Model>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateFinalMp4", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }


        public async Task UpdateFinalMp4(int projectId, int mp4id, int finalmp4_version, string finalmp4_comments, string finalmp4_mediamp4)
        {
            var url = $"api/connect/mp4/{projectId}/videoupdate/{mp4id}";

            var multipart = new MultipartFormDataContent();


            // FK
            var requestbodyContent = new StringContent(finalmp4_version.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"finalmp4_version\"");
            multipart.Add(requestbodyContent);


            // Comments
            requestbodyContent = new StringContent(finalmp4_comments.ToString());
            requestbodyContent.Headers.Add("Content-Disposition", "form-data; name=\"finalmp4_comments\"");
            multipart.Add(requestbodyContent);


            //File
            var currentDirectory = Directory.GetCurrentDirectory();
            var path = $"{currentDirectory}\\Media\\{finalmp4_mediamp4}";
            var fileStream = new FileStream(path, FileMode.Open);

            var fileStreamContent = new StreamContent(fileStream);
            fileStreamContent.Headers.Add("Content-Disposition", $"form-data; name=\"finalmp4_mediamp4\"; filename=\"{finalmp4_mediamp4}\"");
            multipart.Add(fileStreamContent);

            //var result = await _apiClientHelper.Update<ParentData<FinalMp4Model>>(url, multipart);
            var result = await _apiClientHelper.Create<ParentData<FinalMp4Model>>(url, multipart);
            if (result?.Status == "success")
            {
                MessageBox.Show($@"{result?.Message} with data - {result.Data}", "CreateFinalMp4", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            fileStreamContent.Dispose();
            fileStream.Close();
        }

        public async Task DownloadFinalMp4(int FinalMp4Id, string extension)
        {
            var url = $"api/connect/download/video/mp4/{FinalMp4Id}";
            var filename = $"finalmp4_{DateTime.Now:yyyyMMddhhmmss.ffff}.{extension}";
            await _apiClientHelper.GetFile(url, filename);
            MessageBox.Show($@"File Downloaded to path - {Directory.GetCurrentDirectory()}\\{filename}", "CreateFinalMp4", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        #endregion
        */
    }
}
