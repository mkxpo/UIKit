using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core {

    /// <summary>
    /// Провайдер глобальных сервисов приложения
    /// </summary>
    public static class ServiceProvider {

        /// <summary>
        /// Глобальный экземпляр фабрики DbContext
        /// </summary>
        public static IDataContextFactory DataContextFactory;

        /// <summary>
        /// Глобальный экземпляр провайдера аутентификации
        /// </summary>
        public static IAuthenticationProvider AuthenticationProvider;
    }
}
