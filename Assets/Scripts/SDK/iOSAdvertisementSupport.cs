using System;
using System.Threading.Tasks;
using UnityEngine;

public static class iOSAdvertisementSupport
{
    /// <summary>
    /// Запрашивает у пользователя разрешение на отслеживание (только iOS).
    /// На Android и в Editor возвращает CompletedTask.
    /// </summary>
    public static Task RequestTracking()
    {
#if UNITY_IOS && !UNITY_EDITOR
        try
        {
            // ⏳ Для AppsFlyer: задаём таймаут ожидания ответа от ATT (в секундах).
            AppsFlyer.waitForATTUserAuthorizationWithTimeoutInterval(60);

            // Создаём TaskCompletionSource, чтобы связать callback с Task.
            var completionSource = new TaskCompletionSource<bool>();

            // Покажет системный диалог iOS "Разрешить отслеживание?"
            Unity.Advertisement.IosSupport.ATTrackingStatusBinding.RequestAuthorizationTracking(code =>
                AuthorizationTrackingReceived(code, completionSource));

            return completionSource.Task;
        }
        catch (Exception e)
        {
            Debug.LogError($"[ATT] Exception while requesting tracking: {e}");
            return Task.CompletedTask;
        }
#else
        // На Android и в Editor — ничего не делаем
        return Task.CompletedTask;
#endif
    }

#if UNITY_IOS && !UNITY_EDITOR
    private static void AuthorizationTrackingReceived(int code, TaskCompletionSource<bool> completionSource)
    {
        Debug.Log($"[ATT] Authorization status: {code}");

        // Код статуса ATT:
        // 0 = NotDetermined
        // 1 = Restricted
        // 2 = Denied
        // 3 = Authorized

        // Считаем успешным только статус Authorized
        completionSource.TrySetResult(code == 3);
    }
#endif
}
