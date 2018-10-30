﻿using ShopSite.DAL;
using ShopSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopSite.Infrastructure
{
    public class BucketManager
    {
        private ItemsContext db;
        private ISessionManager session;

        public BucketManager(ISessionManager session, ItemsContext db)
        {
            this.session = session;
            this.db = db;
        }

        public List<BucketStatus> DownloadBucket()
        {
            List<BucketStatus> bucket;
            
            if (session.Get<List<BucketStatus>>(Consts.BucketSessionKey)==null)
            {
                bucket = new List<BucketStatus>();
            }
            else
            {
                bucket = session.Get<List<BucketStatus>>(Consts.BucketSessionKey) as List<BucketStatus>;
            }

            return bucket;
        }

        public void AddToBucket(int itemId)
        {
            var bucket = DownloadBucket();
            var bucketStatus = bucket.Find(k => k.Item.ItemId == itemId);

            if(bucketStatus != null)
            {
                bucketStatus.Quantity++;
            }
            else
            {
                var itemToAdd = db.Items.Where(k => k.ItemId == itemId).SingleOrDefault();

                if(itemToAdd != null)
                {
                    var newBucketStatus = new BucketStatus
                    {
                        Item = itemToAdd,
                        Quantity = 1,
                        Value = itemToAdd.ItemPrice
                    };

                    bucket.Add(newBucketStatus);
                }
            }

            session.Set(Consts.BucketSessionKey, bucket);
        }

        public int DeleteFromBucket (int itemId)
        {
            var bucket = DownloadBucket();
            var bucketStatus = bucket.Find(k => k.Item.ItemId == itemId);

            if (bucketStatus != null)
            {
                if (bucketStatus.Quantity > 1)
                {
                    bucketStatus.Quantity--;
                    return bucketStatus.Quantity;
                }
                else
                {
                    bucket.Remove(bucketStatus);
                }
            }

            return 0;
        }

        public decimal GetBucketValues()
        {
            var bucket = DownloadBucket();
            return bucket.Sum(k => (k.Quantity * k.Item.ItemPrice));
        }

        public int GetBucketQuantity()
        {
            var bucket = DownloadBucket();
            int quantity = bucket.Sum(k => k.Quantity);
            return quantity;
        }

        public Order CreateOrder(Order newOrder, string userId)
        {
            var bucket = DownloadBucket();
            newOrder.AdditionDate = DateTime.Now;
            //newOrder.userId = userId;

            db.Orders.Add(newOrder);

            if(newOrder.ItemPosition == null)
            {
                newOrder.ItemPosition = new List<ItemPosition>();
            }

            decimal bucketValue = 0;

            foreach(var bucketElement in bucket)
            {
                var newOrderPosition = new ItemPosition()
                {
                    ItemId = bucketElement.Item.ItemId,
                    Quantity = bucketElement.Quantity,
                    OrderValue = bucketElement.Item.ItemPrice
                };

                bucketValue += (bucketElement.Quantity * bucketElement.Item.ItemPrice);
                newOrder.ItemPosition.Add(newOrderPosition);
            }

            newOrder.OrderValue = bucketValue;
            db.SaveChanges();

            return newOrder;
        }

        public void EmptyBucket()
        {
            session.Set<List<BucketStatus>>(Consts.BucketSessionKey, null);
        }
    }
}