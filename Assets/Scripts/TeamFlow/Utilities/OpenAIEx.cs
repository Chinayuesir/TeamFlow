using Cysharp.Threading.Tasks;
using OpenAI.Threads;
using UnityEngine;

namespace TeamFlow
{
    public static class OpenAIEx
    {
        public static async UniTask<RunResponse> WaitForRunCompleteAsync(this RunResponse run) {
            // wait while it is running. 
            while (run.IsRunning()) {
                run = await run.WaitForStatusChangeAsync(500,90);
                // debug to see steps
                Debug.Log($"[{run.Id}] status: {run.Status} | {run.CreatedAt}");
            }
            // return the response. 
            return run;
        }


        /// <summary> check whether it's still running. </summary>
        static bool IsRunning(this RunResponse runResponse) {
            // check whether it is running still. 
            return (runResponse.Status == RunStatus.Queued
                    || runResponse.Status == RunStatus.InProgress
                    || runResponse.Status == RunStatus.Cancelling);
        }
    }
}