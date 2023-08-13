﻿using SchedulingTool.Api.Domain.Models;
using SchedulingTool.Api.Domain.Services.Communication;
using ModelTask = SchedulingTool.Api.Domain.Models.Task;

namespace SchedulingTool.Api.Domain.Services.Unused;

public interface IViewService
{
  Task<IEnumerable<View>> GetViewsByProjectId( long projectId );
  Task<ServiceResponse<View>> CreateView( View view );
  Task<ServiceResponse<View>> UpdateView( View view );
  Task<View?> GetViewById( long viewId );
  //Task<IEnumerable<object>> GetViewDetailById( View view, IEnumerable<GroupTask> groupTasks, List<ModelTask> tasks );
  //Task<IEnumerable<object>> GetViewDetailById( long projectId, IEnumerable<ViewTaskDetail> tasks );
  //System.Threading.Tasks.Task DeleteView( long viewId, bool isDeleteView );
  Task<IEnumerable<ViewTaskDetail>> GetViewTasks( long projectId, long viewId );
}