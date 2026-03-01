using EcommerceBuisnessLayer.Dtos;
using EcommerceDataLayer;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.Extensions.Logging;
namespace EcommerceBuisnessLayer
{
    public interface IProductRepository
    {
        Task<ProductDTO?> GetProductById(int ProductId);
        Task<int> DecreaseProductStock(int ProductId, int Quantity);
        Task<int> IncreaseProductStock(int ProductId, int Quantity);
    }

    public interface IOrderRepository
    {
        Task<int> CreateOrder(int Userid);
        Task<int> AddOrderItem(int orderId, int ProductId, int Quantity, decimal Price);
        Task<int> UpdateOrderTotal(int orderId, decimal totalAmount);
        Task<int> CancelOrderAsync(int OrderId);
        Task<List<OrderItemRequestDto>> GetOrderItemsForCancel(int OrderId);
    }

    public class ProductRepository : IProductRepository
    {
        public async Task<ProductDTO?> GetProductById(int ProductId) => await ProductsData.GetProductById(ProductId);
        public async Task<int> DecreaseProductStock(int ProductId, int Quantity) => await OrderData.DecreaseProductStock(ProductId, Quantity);
        public async Task<int> IncreaseProductStock(int ProductId, int Quantity) => await OrderData.IncreaseProduct(ProductId, Quantity);
    }

    public class OrderRepository : IOrderRepository
    {
        public async Task<int> CreateOrder(int Userid) => await OrderData.CreatOrder(Userid);
        public async Task<int> AddOrderItem(int orderId, int ProductId, int Quantity, decimal Price) => await OrderData.AddOrderItem(orderId, ProductId, Quantity, Price);
        public async Task<int> UpdateOrderTotal(int orderId, decimal totalAmount) => await OrderData.UpdateOrderTotal(orderId, totalAmount);
        public async Task<int> CancelOrderAsync(int OrderId) => await OrderData.CancelOrder(OrderId);

        public async Task<List<OrderItemRequestDto>> GetOrderItemsForCancel(int OrderId)
        {
            //Mapping
            var DataItems = await OrderData.GetOrderItemsForCancel(OrderId);
            return DataItems.Select(item => new OrderItemRequestDto { ProductId = item.ProductId, Quantity = item.Quantity }).ToList();
        }
    }

    public class OrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ILogger<OrderService> _logger;
        public OrderService(IOrderRepository orderRepository, IProductRepository productRepository, ILogger<OrderService> Logger)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _logger = Logger;
        }

        public enum PlaceOrderResult
        {
            Success = 1,
            InvalidData = 0,
            ProductNotFound = -1,
            InsufficientStock = -2,
            Failed = -99
        }

        public enum CancelOrderResult
        {
            Success = 1,
            OrderNotFound = 0,
            OrderCannotBeCanceled = -3,
            OrderAlreadyCanceled = -2,
            UnexpectedError = -1
        }

        // --- Place Order Logic ---
        public async Task<PlaceOrderResult> PlaceOrder(PlaceOrderRequestDto request)
        {
            _logger.LogInformation("PlaceOrder started. Userid: {Userid}", request?.Userid);

            if (request == null || request.Items == null || !request.Items.Any())
            {
                _logger.LogWarning("PlaceOrder failed validation. Invalid request data.");
                return PlaceOrderResult.InvalidData;
            }

            decimal totalAmount = 0;

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            try
            {
                int orderId = await _orderRepository.CreateOrder(request.Userid);

                if (orderId <= 0)
                {
                    _logger.LogError("PlaceOrder failed. Could not create order for UserId {UserId}", request.Userid);
                    return PlaceOrderResult.Failed;
                }

                _logger.LogInformation("Order created successfully. OrderId: {OrderId}", orderId);
                foreach (var item in request.Items)
                {
                    var product = await _productRepository.GetProductById(item.ProductId);

                    if (product == null) return PlaceOrderResult.ProductNotFound;
                    if (product.Stock < item.Quantity) return PlaceOrderResult.InsufficientStock;

                    int addItemResult = await _orderRepository.AddOrderItem(orderId, item.ProductId, item.Quantity, product.Price);
                    if (addItemResult != 1) return PlaceOrderResult.Failed;

                    int decreaseStockResult = await _productRepository.DecreaseProductStock(item.ProductId, item.Quantity);
                    if (decreaseStockResult != 1) return PlaceOrderResult.InsufficientStock;

                    totalAmount += product.Price * item.Quantity;
                }

                int totalResult = await _orderRepository.UpdateOrderTotal(orderId, totalAmount);
                if (totalResult != 1) return PlaceOrderResult.Failed;

                scope.Complete();
                _logger.LogInformation("PlaceOrder completed successfully. OrderId: {OrderId}", orderId);
                return PlaceOrderResult.Success;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Unhandled exception while placing order.");
                return PlaceOrderResult.Failed;
            }
        }

        // --- Cancel Order Logic (تم دمج المنطق الصحيح هنا) ---
        public async Task<CancelOrderResult> CancelOrderAsync(int OrderId)
        {
            _logger.LogInformation("Cancel Order Started : OrderId {OrderId}", OrderId);

            if (OrderId <= 0) return CancelOrderResult.OrderNotFound;

            // 1. محاولة الإلغاء في قاعدة البيانات
            int CancelResult = await _orderRepository.CancelOrderAsync(OrderId);

            // 2. التحقق من النتيجة (تم نقل Switch Case هنا)
            if (CancelResult != 1) // Assuming 1 is success based on logic context (or check your SP return value)
            {
                // إذا لم يرجع 1 (نجاح)، نفحص باقي القيم
                // ملاحظة: تأكد من القيم التي ترجعها الـ Stored Procedure عندك
                // لو الـ SP بترجع -1 في حالة النجاح، عدل الشرط. 
                // الكود أدناه مبني على أن القيم السالبة هي أخطاء
                if (CancelResult <= 0)
                {
                    return CancelResult switch
                    {
                        0 => CancelOrderResult.OrderNotFound,
                        -2 => CancelOrderResult.OrderAlreadyCanceled,
                        -3 => CancelOrderResult.OrderCannotBeCanceled,
                        _ => CancelOrderResult.UnexpectedError
                    };
                }
            }

            // 3. إرجاع المخزون (فقط في حالة النجاح)
            var OrderItems = await _orderRepository.GetOrderItemsForCancel(OrderId);

            foreach (var item in OrderItems)
            {
                await _productRepository.IncreaseProductStock(item.ProductId, item.Quantity);
            }

            _logger.LogInformation("Order canceled successfully. OrderId: {OrderId}", OrderId);
            return CancelOrderResult.Success;
        }
    }
}